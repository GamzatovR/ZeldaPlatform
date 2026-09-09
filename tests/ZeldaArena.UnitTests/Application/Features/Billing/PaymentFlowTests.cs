using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

/// <summary>
/// Мнимая оплата подписки целиком (docs/SPEC.md §7.6) — то, что показывается
/// на защите: реквизиты, письмо с кодом, подтверждение, активная подписка.
/// </summary>
public class PaymentFlowTests
{
    private readonly PaymentScenarioFixture _fixture = new();

    [Fact]
    public async Task Starting_a_payment_creates_a_pending_record_and_mails_the_code()
    {
        var result = await _fixture.StartAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.MaskedEmail.ShouldBe("p***r@zeldaarena.test");

        var payment = _fixture.SinglePayment;
        payment.Status.ShouldBe(PaymentStatus.Pending);
        payment.Purpose.ShouldBe(PaymentPurpose.Subscription);
        payment.ConfirmationAttemptsLeft.ShouldBe(Payment.MaxAttempts);

        _fixture.Email.Count(RecordingBillingEmailSender.LetterKind.PaymentCode).ShouldBe(1);
        _fixture.SentCode.ShouldBe(_fixture.Codes.Code);
    }

    /// <summary>
    /// Главное требование §7.6 и §20 пункта 6: от карты в базе остаются только
    /// последние четыре цифры и платёжная система.
    /// </summary>
    [Fact]
    public async Task Only_the_last_four_digits_and_the_brand_survive_authorisation()
    {
        await _fixture.StartAsync();

        var payment = _fixture.SinglePayment;

        payment.CardLast4.ShouldBe("4242");
        payment.CardBrand.ShouldBe("Visa");

        // Полного номера нет ни в одном строковом свойстве записи.
        payment.ShouldNotBeNull();
        typeof(Payment).GetProperties()
            .Select(property => property.GetValue(payment) as string)
            .Where(value => value is not null)
            .ShouldAllBe(value => !value!.Contains(PaymentScenarioFixture.ValidCardNumber, StringComparison.Ordinal));
    }

    /// <summary>Код хранится хешем — сам код известен только письму (§7.6).</summary>
    [Fact]
    public async Task The_code_is_stored_hashed_and_never_in_clear_text()
    {
        await _fixture.StartAsync();

        var payment = _fixture.SinglePayment;

        payment.ConfirmationCodeHash.ShouldBe(_fixture.Codes.Hash(_fixture.SentCode));
        payment.ConfirmationCodeHash.ShouldNotBe(_fixture.SentCode);
    }

    /// <summary>Цену определяет тариф, а не форма: значения с клиента не принимаются (§15).</summary>
    [Fact]
    public async Task The_amount_comes_from_the_plan()
    {
        await _fixture.StartAsync();

        _fixture.SinglePayment.Amount.Amount.ShouldBe(PaymentScenarioFixture.Price.Amount);
        _fixture.Gateway.LastRequest.ShouldNotBeNull()
            .Amount.Amount.ShouldBe(PaymentScenarioFixture.Price.Amount);
    }

    [Fact]
    public async Task Confirming_with_the_mailed_code_activates_the_subscription()
    {
        var started = await _fixture.StartAsync();

        var result = await _fixture.ConfirmAsync(_fixture.SentCode, started.Value.PaymentId);

        result.IsSuccess.ShouldBeTrue();
        _fixture.SinglePayment.Status.ShouldBe(PaymentStatus.Succeeded);

        var subscription = _fixture.Subscriptions.Entities.Single();
        subscription.Status.ShouldBe(SubscriptionStatus.Active);
        subscription.PlanId.ShouldBe(_fixture.Monthly.Id);
        subscription.EndsAt.ShouldBe(PaymentScenarioFixture.Start.AddDays(30));

        _fixture.Email.Count(RecordingBillingEmailSender.LetterKind.Receipt).ShouldBe(1);
    }

    /// <summary>Успешный платёж поднимает событие для роли Premium и сброса кэша прав (§7.5, п. 3).</summary>
    [Fact]
    public async Task Successful_payment_raises_the_domain_events()
    {
        await _fixture.PayAsync();

        _fixture.SinglePayment.DomainEvents.OfType<PaymentConfirmedEvent>()
            .ShouldHaveSingleItem();

        _fixture.Subscriptions.Entities.Single()
            .DomainEvents.OfType<SubscriptionActivatedEvent>()
            .ShouldNotBeEmpty();
    }

    /// <summary>
    /// Cookie хранит снимок ролей на момент входа, поэтому без перевыпуска бейдж
    /// Premium появился бы только через пять минут — а человек смотрит на результат
    /// оплаты прямо сейчас. Поймано сквозной проверкой на живом приложении.
    /// </summary>
    [Fact]
    public async Task Successful_payment_refreshes_the_sign_in_cookie()
    {
        await _fixture.PayAsync();

        _fixture.SignIn.RefreshedUsers.ShouldBe([_fixture.UserId]);
    }

    /// <summary>Неудачная оплата ролей не меняет, и трогать cookie незачем.</summary>
    [Fact]
    public async Task A_failed_confirmation_leaves_the_cookie_alone()
    {
        await _fixture.StartAsync();
        await _fixture.ConfirmAsync("000000");

        _fixture.SignIn.RefreshedUsers.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_wrong_code_costs_an_attempt_and_leaves_the_payment_pending()
    {
        await _fixture.StartAsync();

        var result = await _fixture.ConfirmAsync("000000");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(BillingErrors.WrongCode);

        var payment = _fixture.SinglePayment;
        payment.Status.ShouldBe(PaymentStatus.Pending);
        payment.ConfirmationAttemptsLeft.ShouldBe(Payment.MaxAttempts - 1);
    }

    /// <summary>
    /// Израсходованная попытка обязана сохраниться. Иначе счётчик не убывал бы
    /// и код подбирался бы бесконечно.
    /// </summary>
    [Fact]
    public async Task A_wrong_code_is_persisted_so_the_attempt_is_really_spent()
    {
        await _fixture.StartAsync();

        var before = _fixture.UnitOfWork.SaveChangesCalls;

        await _fixture.ConfirmAsync("000000");

        _fixture.UnitOfWork.SaveChangesCalls.ShouldBeGreaterThan(before);
    }

    [Fact]
    public async Task Running_out_of_attempts_fails_the_payment()
    {
        await _fixture.StartAsync();

        for (var attempt = 0; attempt < Payment.MaxAttempts - 1; attempt++)
        {
            (await _fixture.ConfirmAsync("000000")).Error.ShouldBe(BillingErrors.WrongCode);
        }

        var last = await _fixture.ConfirmAsync("000000");

        last.Error.ShouldBe(BillingErrors.NoAttemptsLeft);
        _fixture.SinglePayment.Status.ShouldBe(PaymentStatus.Failed);
    }

    [Fact]
    public async Task An_expired_code_fails_the_payment()
    {
        await _fixture.StartAsync();

        _fixture.Advance(PaymentPolicy.CodeLifetime + TimeSpan.FromSeconds(1));

        var result = await _fixture.ConfirmAsync(_fixture.SentCode);

        result.Error.ShouldBe(BillingErrors.CodeExpired);
        _fixture.SinglePayment.Status.ShouldBe(PaymentStatus.Failed);
    }

    /// <summary>
    /// Заявка на подписку была носителем выбранного тарифа. Оплата не состоялась —
    /// она не должна остаться в базе мусором.
    /// </summary>
    [Fact]
    public async Task A_failed_payment_discards_the_reserved_subscription()
    {
        await _fixture.StartAsync();
        _fixture.Subscriptions.Entities.ShouldHaveSingleItem();

        _fixture.Advance(PaymentPolicy.CodeLifetime + TimeSpan.FromSeconds(1));
        await _fixture.ConfirmAsync(_fixture.SentCode);

        _fixture.Subscriptions.Entities.ShouldBeEmpty();
    }

    /// <summary>Права даёт только активная подписка: заявка в ожидании — ещё не оплата.</summary>
    [Fact]
    public async Task A_reserved_subscription_grants_nothing_until_it_is_paid()
    {
        await _fixture.StartAsync();

        var reserved = _fixture.Subscriptions.Entities.Single();

        reserved.Status.ShouldBe(SubscriptionStatus.Pending);
        reserved.IsActiveAt(PaymentScenarioFixture.Start).ShouldBeFalse();
    }

    [Fact]
    public async Task A_confirmed_payment_cannot_be_confirmed_twice()
    {
        await _fixture.PayAsync();

        var again = await _fixture.ConfirmAsync(_fixture.Codes.Code);

        again.Error.ShouldBe(BillingErrors.AlreadyProcessed);
        _fixture.Subscriptions.Entities.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task A_declined_card_creates_no_payment()
    {
        _fixture.Gateway.Declines = true;

        var result = await _fixture.StartAsync();

        result.Error.ShouldBe(BillingErrors.CardDeclined);
        _fixture.Payments.Entities.ShouldBeEmpty();
        _fixture.Subscriptions.Entities.ShouldBeEmpty();
        _fixture.Email.Letters.ShouldBeEmpty();
    }

    [Fact]
    public async Task The_free_plan_cannot_be_purchased()
    {
        var result = await _fixture.StartAsync(_fixture.Free.Id);

        result.Error.ShouldBe(BillingErrors.PlanNotPurchasable);
    }

    [Fact]
    public async Task A_withdrawn_plan_cannot_be_purchased()
    {
        _fixture.Monthly.Deactivate();

        var result = await _fixture.StartAsync();

        result.Error.ShouldBe(BillingErrors.PlanInactive);
    }

    /// <summary>
    /// Идемпотентность из §7.6: обновлённая страница и второй клик не заводят
    /// второй платёж и не шлют второе письмо.
    /// </summary>
    [Fact]
    public async Task The_same_idempotency_key_returns_the_same_payment()
    {
        var first = await _fixture.StartAsync();
        var second = await _fixture.StartAsync();

        second.Value.PaymentId.ShouldBe(first.Value.PaymentId);
        _fixture.Payments.Entities.ShouldHaveSingleItem();
        _fixture.Email.Count(RecordingBillingEmailSender.LetterKind.PaymentCode).ShouldBe(1);
    }

    /// <summary>
    /// Ключ приходит скрытым полем формы, то есть подконтролен клиенту, а уникальный
    /// индекс на нём — общий на всю таблицу (§6). Пока владелец не был частью ключа,
    /// чужое значение роняло вставку на нарушении уникальности: поиск шёл по паре
    /// «пользователь + ключ» и существующий платёж не находил. Поймано пробой
    /// на живом приложении — приходил 500.
    /// </summary>
    [Fact]
    public async Task Two_users_may_submit_the_same_idempotency_key()
    {
        await _fixture.StartAsync(idempotencyKey: "form-1");

        var firstKey = _fixture.SinglePayment.IdempotencyKey;

        _fixture.SignedInUserId = Guid.CreateVersion7();
        var second = await _fixture.StartAsync(idempotencyKey: "form-1");

        second.IsSuccess.ShouldBeTrue();
        _fixture.Payments.Entities.Count.ShouldBe(2);

        // Ключи в базе разные, поэтому глобальный уникальный индекс не сработает.
        _fixture.Payments.Entities[^1].IdempotencyKey.ShouldNotBe(firstKey);
    }

    [Fact]
    public async Task A_different_idempotency_key_starts_a_new_payment()
    {
        await _fixture.StartAsync(idempotencyKey: "form-1");
        await _fixture.StartAsync(idempotencyKey: "form-2");

        _fixture.Payments.Entities.Count.ShouldBe(2);
    }

    /// <summary>Чужой платёж отвечает так же, как несуществующий (§15, защита от IDOR).</summary>
    [Fact]
    public async Task Another_user_cannot_confirm_someone_elses_payment()
    {
        var started = await _fixture.StartAsync();

        _fixture.SignedInUserId = Guid.CreateVersion7();

        var result = await _fixture.ConfirmAsync(_fixture.SentCode, started.Value.PaymentId);

        result.Error.ShouldBe(BillingErrors.PaymentNotFound);
        _fixture.SinglePayment.Status.ShouldBe(PaymentStatus.Pending);
    }

    [Fact]
    public async Task Another_user_cannot_read_someone_elses_payment_state()
    {
        var started = await _fixture.StartAsync();

        _fixture.SignedInUserId = Guid.CreateVersion7();

        (await _fixture.StateAsync(started.Value.PaymentId)).ShouldBeNull();
    }

    [Fact]
    public async Task Cancelling_removes_the_reservation_and_closes_the_payment()
    {
        await _fixture.StartAsync();

        var result = await _fixture.CancelAsync();

        result.IsSuccess.ShouldBeTrue();
        _fixture.SinglePayment.Status.ShouldBe(PaymentStatus.Canceled);
        _fixture.Subscriptions.Entities.ShouldBeEmpty();
    }
}