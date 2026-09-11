using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;

/// <summary>Заказ целиком для страницы <c>/orders/{number}</c>.</summary>
public sealed record OrderDetailsDto
{
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

    public string Country { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string Street { get; init; } = string.Empty;

    public string PostalCode { get; init; } = string.Empty;

    public IReadOnlyList<OrderLineDto> Lines { get; init; } = [];

    /// <summary>Платёж, который ещё ждёт кода, — чтобы вернуться к его вводу.</summary>
    public Guid? PendingPaymentId { get; init; }

    /// <summary>Покупатель отменяет только неоплаченный заказ (docs/adr/ADR-0009).</summary>
    public bool CanCancel => Status == OrderStatus.Pending;

    public bool CanContinuePayment => Status == OrderStatus.Pending && PendingPaymentId is not null;
}