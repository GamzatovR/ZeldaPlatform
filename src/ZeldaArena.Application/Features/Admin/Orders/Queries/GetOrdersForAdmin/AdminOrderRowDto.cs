using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrdersForAdmin;

public sealed record AdminOrderRowDto
{
    public string Number { get; init; } = string.Empty;

    public OrderStatus Status { get; init; }

    public DateTimeOffset PlacedAt { get; init; }

    public decimal Total { get; init; }

    public string Currency { get; init; } = string.Empty;

    public int ItemCount { get; init; }

    public string Recipient { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;
}