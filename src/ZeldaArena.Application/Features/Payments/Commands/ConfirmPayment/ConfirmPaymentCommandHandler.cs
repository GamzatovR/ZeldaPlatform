using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Features.Orders;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;

/// <summary>
/// Сверяет код и, если он верен, доводит покупку до конца — в одной транзакции,
/// как требует docs/SPEC.md §7.5, п. 3: подписка активируется или продлевается,
/// заказ помечается оплаченным (§7.6, шаг 4).
///
/// Неудачная попытка тоже сохраняется. Это не мелочь: <c>Payment.Confirm</c>
/// уменьшает счётчик попыток, и если бы неуспех откатывал транзакцию, счётчик
/// не убывал бы и код можно было бы подбирать бесконечно. Транзакция коммитится
/// при <c>Result.Failure</c> и откатывается только исключением — на это здесь и расчёт.
///
/// Выдачу роли Premium, сброс кэша прав и уведомление делают обработчики
/// <c>SubscriptionActivatedEvent</c>: хендлер занимается деньгами и сроком (§5.5, SRP).
/// </summary>
public sealed class ConfirmPaymentCommandHandler(
    ICurrentUserService currentUser,
    IRepository<Payment> payments,
    IRepository<Subscription> subscriptions,
    IReadRepository<Subscription> subscriptionsForRead,
    IReadRepository<Plan> plans,
    IRepository<Order> orders,
    OrderCancellation orderCancellation,
    IQueryExecutor queryExecutor,
    IConfirmationCodeProtector codes,
    IBillingEmailSender emailSender,
    ISignInService signInService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
    : IRequestHandler<ConfirmPaymentCommand, Result>
{
    public async Task<Result> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var payment = await payments
            .GetByIdAsync(request.PaymentId, cancellationToken)
            .ConfigureAwait(false);

        // Чужой платёж отвечает «не найден», а не «не ваш»: подтверждать существование
        // чужой записи незачем.
        if (payment is null || payment.UserId != userId)
        {
            return Result.Failure(BillingErrors.PaymentNotFound);
        }

        return payment.Purpose == PaymentPurpose.Order
            ? await ConfirmOrderPaymentAsync(payment, request.ConfirmationCode, cancellationToken).ConfigureAwait(false)
            : await ConfirmSubscriptionPaymentAsync(payment, request.ConfirmationCode, userId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result> ConfirmSubscriptionPaymentAsync(
        Payment payment,
        string confirmationCode,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var outcome = payment.Confirm(codes.Hash(confirmationCode), now);

        if (outcome != PaymentConfirmationResult.Succeeded)
        {
            if (IsTerminal(outcome))
            {
                await DiscardReservationAsync(payment, cancellationToken).ConfigureAwait(false);
            }

            return await FailAsync(outcome, cancellationToken).ConfigureAwait(false);
        }

        var activated = await ActivateSubscriptionAsync(payment, userId, now, cancellationToken)
            .ConfigureAwait(false);

        if (activated.IsFailure)
        {
            return Result.Failure(activated.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Роль Premium выдана обработчиком события, но в cookie её ещё нет: та хранит
        // снимок ролей на момент входа и обновилась бы сама только через пять минут
        // (SecurityStampValidationInterval). Пользователь смотрит на результат оплаты
        // прямо сейчас, поэтому cookie перевыписывается здесь — как после смены пароля
        // в Фазе 3.
        await signInService.RefreshSignInAsync(userId, cancellationToken).ConfigureAwait(false);

        await emailSender
            .SendPaymentReceiptAsync(
                payment.ConfirmationEmail,
                currentUser.UserName,
                activated.Value.PlanName,
                payment.Amount.Amount,
                payment.Amount.Currency,
                activated.Value.EndsAt,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }

    /// <summary>
    /// Заказ оплачивается, только пока ждёт оплаты. Если его успели отменить в соседней
    /// вкладке, код не принимается вовсе: иначе деньги списались бы за отменённый заказ.
    ///
    /// Истёкший код и исчерпанные попытки сразу отменяют заказ и возвращают остаток —
    /// правило ADR-0009: повторная оплата — только новым оформлением.
    /// </summary>
    private async Task<Result> ConfirmOrderPaymentAsync(
        Payment payment,
        string confirmationCode,
        CancellationToken cancellationToken)
    {
        var order = payment.OrderId is { } orderId
            ? await orders.GetByIdAsync(orderId, cancellationToken).ConfigureAwait(false)
            : null;

        if (order is null)
        {
            return Result.Failure(ShopErrors.OrderNotFound);
        }

        if (payment.IsPending && order.Status != OrderStatus.Pending)
        {
            payment.Fail(ShopErrors.OrderNotPayable.Code);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Failure(ShopErrors.OrderNotPayable);
        }

        var now = clock.UtcNow;
        var outcome = payment.Confirm(codes.Hash(confirmationCode), now);

        if (outcome != PaymentConfirmationResult.Succeeded)
        {
            if (IsTerminal(outcome) && order.Status == OrderStatus.Pending)
            {
                await orderCancellation.CancelAsync(order, returnItemsToCart: true, cancellationToken).ConfigureAwait(false);
            }

            return await FailAsync(outcome, cancellationToken).ConfigureAwait(false);
        }

        order.MarkPaid(now);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await emailSender
            .SendOrderReceiptAsync(
                payment.ConfirmationEmail,
                currentUser.UserName,
                order.Number,
                order.Total,
                order.Currency,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }

    /// <summary>Исчерпанные попытки и истёкший срок роняют платёж в Failed; неверный код — нет.</summary>
    private static bool IsTerminal(PaymentConfirmationResult outcome) =>
        outcome is PaymentConfirmationResult.Expired or PaymentConfirmationResult.NoAttemptsLeft;

    private async Task<Result> FailAsync(PaymentConfirmationResult outcome, CancellationToken cancellationToken)
    {
        // Сохраняем и при неуспехе: в записи изменился счётчик попыток или статус.
        if (outcome != PaymentConfirmationResult.AlreadyProcessed)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Failure(outcome switch
        {
            PaymentConfirmationResult.WrongCode => BillingErrors.WrongCode,
            PaymentConfirmationResult.Expired => BillingErrors.CodeExpired,
            PaymentConfirmationResult.NoAttemptsLeft => BillingErrors.NoAttemptsLeft,
            _ => BillingErrors.AlreadyProcessed,
        });
    }

    /// <summary>
    /// Один путь на оба случая из §7.5, п. 3. Если действующая подписка уже есть,
    /// продлевается она, а заявка удаляется: двух подписок у одного человека быть
    /// не должно. Если нет — активируется сама заявка.
    ///
    /// Продление считает срок от текущего конца, а не от «сегодня», поэтому
    /// оплаченные дни не сгорают при переходе на другой тариф (docs/adr/ADR-0005).
    /// </summary>
    private async Task<Result<ActivatedSubscription>> ActivateSubscriptionAsync(
        Payment payment,
        Guid userId,
        DateTimeOffset paidAt,
        CancellationToken cancellationToken)
    {
        if (payment.SubscriptionId is not { } reservationId)
        {
            return Result.Failure<ActivatedSubscription>(BillingErrors.SubscriptionNotFound);
        }

        var reservation = await subscriptions
            .GetByIdAsync(reservationId, cancellationToken)
            .ConfigureAwait(false);

        if (reservation is null)
        {
            return Result.Failure<ActivatedSubscription>(BillingErrors.SubscriptionNotFound);
        }

        var plan = await plans.FindAsync(reservation.PlanId, cancellationToken).ConfigureAwait(false);

        if (plan is null)
        {
            return Result.Failure<ActivatedSubscription>(BillingErrors.PlanNotFound);
        }

        var existingId = await FindActiveSubscriptionIdAsync(
                userId,
                reservationId,
                paidAt,
                cancellationToken)
            .ConfigureAwait(false);

        if (existingId is null)
        {
            reservation.Extend(plan, paidAt);

            return Result.Success(new ActivatedSubscription(plan.Name, reservation.EndsAt));
        }

        var existing = await subscriptions
            .GetByIdAsync(existingId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.Failure<ActivatedSubscription>(BillingErrors.SubscriptionNotFound);
        }

        existing.Extend(plan, paidAt);
        payment.AttachSubscription(existing.Id);
        subscriptions.Remove(reservation);

        return Result.Success(new ActivatedSubscription(plan.Name, existing.EndsAt));
    }

    /// <summary>
    /// Провал оплаты лишает смысла заявку на подписку — она была только носителем
    /// выбранного тарифа.
    /// </summary>
    private async Task DiscardReservationAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (payment.SubscriptionId is not { } reservationId)
        {
            return;
        }

        var reservation = await subscriptions
            .GetByIdAsync(reservationId, cancellationToken)
            .ConfigureAwait(false);

        // Удаляется только неоплаченная заявка. Действующая подписка, к которой платёж
        // мог быть привязан продлением, остаётся нетронутой.
        if (reservation is { Status: SubscriptionStatus.Pending })
        {
            subscriptions.Remove(reservation);
        }
    }

    private Task<Guid?> FindActiveSubscriptionIdAsync(
        Guid userId,
        Guid reservationId,
        DateTimeOffset moment,
        CancellationToken cancellationToken)
    {
        var query = subscriptionsForRead.Query()
            .Where(subscription => subscription.UserId == userId
                && subscription.Id != reservationId
                && subscription.Status == SubscriptionStatus.Active
                && subscription.EndsAt > moment)
            .OrderByDescending(subscription => subscription.EndsAt)
            .Select(subscription => (Guid?)subscription.Id);

        return queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
    }

    private sealed record ActivatedSubscription(string PlanName, DateTimeOffset EndsAt);
}