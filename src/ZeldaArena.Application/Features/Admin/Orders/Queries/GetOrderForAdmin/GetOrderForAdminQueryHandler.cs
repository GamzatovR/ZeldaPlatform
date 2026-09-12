using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

public sealed class GetOrderForAdminQueryHandler(
    IReadRepository<Order> orders,
    IReadRepository<Payment> payments,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetOrderForAdminQuery, AdminOrderDetailsDto?>
{
    public async Task<AdminOrderDetailsDto?> Handle(GetOrderForAdminQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var number = request.Number.Trim();

        var order = await queryExecutor.FirstOrDefaultAsync(
            orders.Query()
                .Where(item => item.Number == number)
                .Select(item => new AdminOrderDetailsDto
                {
                    Id = item.Id,
                    Number = item.Number,
                    Status = item.Status,
                    PlacedAt = item.PlacedAt,
                    PaidAt = item.PaidAt,
                    CanceledAt = item.CanceledAt,
                    Subtotal = item.Subtotal,
                    DiscountAmount = item.DiscountAmount,
                    Total = item.Total,
                    Currency = item.Currency,
                    Recipient = item.Address.Recipient,
                    Phone = item.Address.Phone,
                    Address = item.Address.Country + ", " + item.Address.City + ", "
                        + item.Address.Street + ", " + item.Address.PostalCode,
                    Items = item.Items
                        .Select(line => new AdminOrderItemDto(
                            line.ProductNameSnapshot,
                            line.UnitPrice,
                            line.Quantity,
                            line.UnitPrice * line.Quantity))
                        .ToList(),
                }),
            cancellationToken);

        if (order is null)
        {
            return null;
        }

        var orderPayments = await queryExecutor.ToListAsync(
            payments.Query()
                .Where(payment => payment.Purpose == PaymentPurpose.Order && payment.OrderId == order.Id)
                .OrderByDescending(payment => payment.CreatedAt)
                .Select(payment => new AdminOrderPaymentDto(
                    payment.Status,
                    payment.Amount.Amount,
                    payment.Amount.Currency,
                    payment.CardBrand,
                    payment.CardLast4,
                    payment.CreatedAt,
                    payment.PaidAt)),
            cancellationToken);

        // Адрес для чека — единственное место, где виден контакт покупателя;
        // в таблице пользователей он и так есть у администратора.
        var email = await queryExecutor.FirstOrDefaultAsync(
            payments.Query()
                .Where(payment => payment.Purpose == PaymentPurpose.Order && payment.OrderId == order.Id)
                .OrderByDescending(payment => payment.CreatedAt)
                .Select(payment => payment.ConfirmationEmail),
            cancellationToken);

        return order with { Payments = orderPayments, CustomerEmail = email };
    }
}