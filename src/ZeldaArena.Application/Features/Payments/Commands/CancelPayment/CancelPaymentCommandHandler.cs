using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Orders;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Payments.Commands.CancelPayment;

/// <summary>
/// Отменяет неоплаченный платёж (docs/SPEC.md §7.6) вместе с тем, ради чего он заводился:
/// заявка на подписку удаляется — она была только носителем выбранного тарифа, —
/// заказ отменяется и возвращает остаток (docs/adr/ADR-0009).
/// </summary>
public sealed class CancelPaymentCommandHandler(
    ICurrentUserService currentUser,
    IRepository<Payment> payments,
    IRepository<Subscription> subscriptions,
    IRepository<Order> orders,
    OrderCancellation orderCancellation,
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

        if (payment.OrderId is { } orderId)
        {
            var order = await orders.GetByIdAsync(orderId, cancellationToken).ConfigureAwait(false);

            if (order is { Status: OrderStatus.Pending })
            {
                await orderCancellation.CancelAsync(order, returnItemsToCart: true, cancellationToken).ConfigureAwait(false);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}