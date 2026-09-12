using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders.Commands.ExpireAbandonedOrders;

public sealed class ExpireAbandonedOrdersCommandHandler(
    IReadRepository<Payment> paymentsForRead,
    IRepository<Payment> payments,
    IRepository<Order> orders,
    OrderCancellation cancellation,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
    : IRequestHandler<ExpireAbandonedOrdersCommand, Result<int>>
{
    public const string FailureReason = "payment.code_expired";

    public async Task<Result<int>> Handle(ExpireAbandonedOrdersCommand request, CancellationToken cancellationToken)
    {
        var threshold = clock.UtcNow - OrderRules.AbandonedAfterCodeExpiry;

        var abandonedIds = await queryExecutor
            .ToListAsync(
                paymentsForRead.Query()
                    .Where(payment => payment.Purpose == PaymentPurpose.Order
                        && payment.Status == PaymentStatus.Pending
                        && payment.ConfirmationExpiresAt < threshold)
                    .Select(payment => payment.Id),
                cancellationToken)
            .ConfigureAwait(false);

        if (abandonedIds.Count == 0)
        {
            return Result.Success(0);
        }

        var canceled = 0;

        foreach (var payment in await payments.GetByIdsAsync([.. abandonedIds], cancellationToken).ConfigureAwait(false))
        {
            payment.Fail(FailureReason);

            var order = payment.OrderId is { } orderId
                ? await orders.GetByIdAsync(orderId, cancellationToken).ConfigureAwait(false)
                : null;

            if (order is { Status: OrderStatus.Pending })
            {
                await cancellation.CancelAsync(order, returnItemsToCart: true, cancellationToken).ConfigureAwait(false);
                canceled++;
            }

            // Сохранение на каждый заказ, а не одно в конце.
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success(canceled);
    }
}