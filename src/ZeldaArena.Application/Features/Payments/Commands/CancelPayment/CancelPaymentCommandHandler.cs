using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Payments.Commands.CancelPayment;

/// <summary>
/// Отменяет неоплаченный платёж и убирает заявку на подписку, которая была всего лишь
/// носителем выбранного тарифа (docs/SPEC.md §7.6).
/// </summary>
public sealed class CancelPaymentCommandHandler(
    ICurrentUserService currentUser,
    IRepository<Payment> payments,
    IRepository<Subscription> subscriptions,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelPaymentCommand, Result>
{
    public async Task<Result> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var payment = await payments
            .GetByIdAsync(request.PaymentId, cancellationToken)
            .ConfigureAwait(false);

        if (payment is null || payment.UserId != userId)
        {
            return Result.Failure(BillingErrors.PaymentNotFound);
        }

        if (!payment.IsPending)
        {
            return Result.Failure(BillingErrors.PaymentNotPending);
        }

        payment.CancelByUser();

        if (payment.SubscriptionId is { } reservationId)
        {
            var reservation = await subscriptions
                .GetByIdAsync(reservationId, cancellationToken)
                .ConfigureAwait(false);

            // Удаляется только неоплаченная заявка: действующая подписка,
            // которую платёж собирался продлить, остаётся нетронутой.
            if (reservation is { Status: SubscriptionStatus.Pending })
            {
                subscriptions.Remove(reservation);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}