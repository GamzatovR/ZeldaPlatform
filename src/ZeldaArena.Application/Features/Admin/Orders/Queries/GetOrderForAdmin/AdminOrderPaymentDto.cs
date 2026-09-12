using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

public sealed record AdminOrderPaymentDto(
    PaymentStatus Status,
    decimal Amount,
    string Currency,
    string CardBrand,
    string CardLast4,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt);