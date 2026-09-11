using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;

/// <summary>Строка истории заказов.</summary>
public sealed record OrderListItemDto
{
    public string Number { get; init; } = string.Empty;

    public OrderStatus Status { get; init; }

    public DateTimeOffset PlacedAt { get; init; }

    public decimal Total { get; init; }

    public string Currency { get; init; } = string.Empty;

    /// <summary>Штук во всех позициях заказа.</summary>
    public int ItemCount { get; init; }
}