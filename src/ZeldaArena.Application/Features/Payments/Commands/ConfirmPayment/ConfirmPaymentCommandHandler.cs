using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;

/// <summary>
/// Сверяет код и, если он верен, активирует или продлевает подписку — всё в одной
/// транзакции, как требует docs/SPEC.md §7.5, п. 3.
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
    IQueryExecutor queryExecutor,
    IConfirmationCodeProtector codes,
    IBillingEmailSender emailSender,
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

        var now = clock.UtcNow;
        var outcome = payment.Confirm(codes.Hash(request.ConfirmationCode), now);

        if (outcome != PaymentConfirmationResult.Succeeded)
        {
            return await FailAsync(payment, outcome, cancellationToken).ConfigureAwait(false);
        }

        var activated = await ActivateSubscriptionAsync(payment, userId, now, cancellationToken)
            .ConfigureAwait(false);

        if (activated.IsFailure)
        {
            return Result.Failure(activated.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
    /// Исчерпанные попытки и истёкший срок роняют платёж в Failed, и вместе с ним
    /// теряет смысл заявка на подписку — она была только носителем выбранного тарифа.
    /// Неверный код такой развязки не требует: попытки ещё остались.
    /// </summary>
    private async Task<Result> FailAsync(
        Payment payment,
        PaymentConfirmationResult outcome,
        CancellationToken cancellationToken)
    {
        if (outcome is PaymentConfirmationResult.Expired or PaymentConfirmationResult.NoAttemptsLeft)
        {
            await DiscardReservationAsync(payment, cancellationToken).ConfigureAwait(false);
        }

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