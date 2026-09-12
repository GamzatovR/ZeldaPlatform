using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

/// <summary>
/// Платёж по заказу. От карты здесь только последние четыре цифры и платёжная система —
/// больше о ней не знает и сама база (docs/SPEC.md §7.6).
/// </summary>
public sealed record AdminOrderPaymentDto(
    PaymentStatus Status,
    decimal Amount,
    string Currency,
    string CardBrand,
    string CardLast4,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt);