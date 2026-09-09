using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

/// <summary>
/// Повторная отправка кода (docs/SPEC.md §7.6, шаг 3): письмо не дошло, попало
/// в спам или код истёк.
/// </summary>
public class PaymentCodeResendTests
{
    private readonly PaymentScenarioFixture _fixture = new();

    /// <summary>
    /// Пауза привязана к платежу, а не к адресу клиента, поэтому её не обойти
    /// вторым окном браузера.
    /// </summary>
    [Fact]
    public async Task A_second_code_cannot_be_requested_within_a_minute()
    {
        await _fixture.StartAsync();

        var result = await _fixture.ResendAsync();

        result.Error.ShouldBe(BillingErrors.ResendTooSoon);
        _fixture.Email.Count(RecordingBillingEmailSender.LetterKind.PaymentCode).ShouldBe(1);
    }

    [Fact]
    public async Task After_the_cooldown_a_new_code_is_mailed()
    {
        await _fixture.StartAsync();

        _fixture.Advance(PaymentPolicy.ResendCooldown);
        _fixture.Codes.NextCodeWillBe("654321");

        var result = await _fixture.ResendAsync();

        result.IsSuccess.ShouldBeTrue();
        _fixture.Email.Count(RecordingBillingEmailSender.LetterKind.PaymentCode).ShouldBe(2);
        _fixture.Email.Letters[^1].Code.ShouldBe("654321");
    }

    /// <summary>Прежний код обязан перестать работать — иначе действующих кодов стало бы два.</summary>
    [Fact]
    public async Task The_previous_code_stops_working()
    {
        await _fixture.StartAsync();

        var oldCode = _fixture.SentCode;

        _fixture.Advance(PaymentPolicy.ResendCooldown);
        _fixture.Codes.NextCodeWillBe("654321");
        await _fixture.ResendAsync();

        (await _fixture.ConfirmAsync(oldCode)).Error.ShouldBe(BillingErrors.WrongCode);
        (await _fixture.ConfirmAsync("654321")).IsSuccess.ShouldBeTrue();
    }

    /// <summary>Новый код — новая серия попыток и новый десятиминутный срок.</summary>
    [Fact]
    public async Task A_new_code_restores_the_attempts_and_the_lifetime()
    {
        await _fixture.StartAsync();
        await _fixture.ConfirmAsync("000000");

        _fixture.SinglePayment.ConfirmationAttemptsLeft.ShouldBe(Payment.MaxAttempts - 1);

        _fixture.Advance(PaymentPolicy.ResendCooldown);
        await _fixture.ResendAsync();

        var payment = _fixture.SinglePayment;
        payment.ConfirmationAttemptsLeft.ShouldBe(Payment.MaxAttempts);
        payment.ConfirmationExpiresAt.ShouldBe(
            PaymentScenarioFixture.Start + PaymentPolicy.ResendCooldown + PaymentPolicy.CodeLifetime);
    }

    /// <summary>
    /// Код высылается только по платежу в ожидании: оплаченный подтверждать нечем,
    /// а по неудавшемуся оплату начинают заново.
    /// </summary>
    [Fact]
    public async Task A_settled_payment_gets_no_new_code()
    {
        await _fixture.PayAsync();

        _fixture.Advance(PaymentPolicy.ResendCooldown);

        (await _fixture.ResendAsync()).Error.ShouldBe(BillingErrors.PaymentNotPending);
    }

    [Fact]
    public async Task Another_user_cannot_trigger_a_resend()
    {
        await _fixture.StartAsync();
        _fixture.Advance(PaymentPolicy.ResendCooldown);

        _fixture.SignedInUserId = Guid.CreateVersion7();

        (await _fixture.ResendAsync()).Error.ShouldBe(BillingErrors.PaymentNotFound);
        _fixture.Email.Count(RecordingBillingEmailSender.LetterKind.PaymentCode).ShouldBe(1);
    }

    /// <summary>Страница ввода кода узнаёт остаток попыток и готовность к повторной отправке отсюда.</summary>
    [Fact]
    public async Task The_state_reports_the_attempts_and_the_resend_readiness()
    {
        await _fixture.StartAsync();
        await _fixture.ConfirmAsync("000000");

        var before = await _fixture.StateAsync();

        before.ShouldNotBeNull();
        before.AttemptsLeft.ShouldBe(Payment.MaxAttempts - 1);
        before.MaskedEmail.ShouldBe("p***r@zeldaarena.test");
        before.CanResendNow.ShouldBeFalse();

        _fixture.Advance(PaymentPolicy.ResendCooldown);

        (await _fixture.StateAsync()).ShouldNotBeNull().CanResendNow.ShouldBeTrue();
    }
}