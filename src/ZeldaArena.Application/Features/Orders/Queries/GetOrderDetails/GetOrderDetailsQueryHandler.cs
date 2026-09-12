using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;

public sealed class GetOrderDetailsQueryHandler(
    ICurrentUserService currentUser,
    IReadRepository<Order> orders,
    IReadRepository<Payment> payments,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetOrderDetailsQuery, OrderDetailsDto?>
{
    public async Task<OrderDetailsDto?> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return null;
        }

        var order = await queryExecutor
            .FirstOrDefaultAsync(orders.OwnedBy(userId, request.Number), cancellationToken)
            .ConfigureAwait(false);

        if (order is null)
        {
            return null;
        }

        var pendingPaymentId = order.Status == OrderStatus.Pending
            ? await queryExecutor
                .FirstOrDefaultAsync(
                    payments.Query()
                        .Where(payment => payment.OrderId == order.Id && payment.Status == PaymentStatus.Pending)
                        .Select(payment => (Guid?)payment.Id),
                    cancellationToken)
                .ConfigureAwait(false)
            : null;

        return new OrderDetailsDto
        {
            Number = order.Number,
            Status = order.Status,
            PlacedAt = order.PlacedAt,
            PaidAt = order.PaidAt,
            CanceledAt = order.CanceledAt,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            Currency = order.Currency,
            Recipient = order.Address.Recipient,
            Phone = order.Address.Phone,
            Country = order.Address.Country,
            City = order.Address.City,
            Street = order.Address.Street,
            PostalCode = order.Address.PostalCode,
            Lines = [.. order.Items.Select(item => new OrderLineDto(item.ProductNameSnapshot, item.UnitPrice, item.Quantity))],
            PendingPaymentId = pendingPaymentId,
        };
    }
}