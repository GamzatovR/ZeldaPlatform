using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

public sealed record AdminOrderDetailsDto
{
    public Guid Id { get; init; }

    public string Number { get; init; } = string.Empty;

    public OrderStatus Status { get; init; }

    public DateTimeOffset PlacedAt { get; init; }

    public DateTimeOffset? PaidAt { get; init; }

    public DateTimeOffset? CanceledAt { get; init; }

    public decimal Subtotal { get; init; }

    public decimal DiscountAmount { get; init; }

    public decimal Total { get; init; }

    public string Currency { get; init; } = string.Empty;

    public string Recipient { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string? CustomerEmail { get; init; }

    public IReadOnlyList<AdminOrderItemDto> Items { get; init; } = [];

    public IReadOnlyList<AdminOrderPaymentDto> Payments { get; init; } = [];

    public bool CanShip => Status == OrderStatus.Paid;

    public bool CanComplete => Status == OrderStatus.Shipped;

    /// <summary>Отменить можно до отправки: отправленный заказ возвращают, а не отменяют.</summary>
    public bool CanCancel => Status is OrderStatus.Pending or OrderStatus.Paid;
}