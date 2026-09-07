using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Shop;

/// <summary>
/// Заказ магазина. Суммы пересчитываются внутри сущности при каждом изменении состава,
/// поэтому итог не может разойтись с позициями и не зависит от данных клиента
/// (docs/SPEC.md §15, §20 п. 7).
/// </summary>
public class Order : BaseEntity, IAuditableEntity
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    /// <summary>Человекочитаемый номер вида ZA-2025-000123. Уникален.</summary>
    public string Number { get; private set; } = null!;

    public Guid UserId { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal Subtotal { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public decimal Total { get; private set; }

    public string Currency { get; private set; } = null!;

    public ShippingAddress Address { get; private set; } = null!;

    public DateTimeOffset PlacedAt { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public DateTimeOffset? CanceledAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money TotalMoney => new(Total, Currency);

    public bool IsPaid => PaidAt is not null;

    public static Order Place(
        Guid userId,
        string number,
        ShippingAddress address,
        IEnumerable<OrderLine> lines,
        DateTimeOffset placedAt,
        decimal discountAmount = 0m,
        string currency = Money.DefaultCurrency)
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);

        InvariantViolationException.ThrowIf(
            userId == Guid.Empty,
            "order.user_required",
            "У заказа обязан быть владелец.");

        var order = new Order
        {
            UserId = userId,
            Number = number.Trim(),
            Address = address,
            Status = OrderStatus.Pending,
            PlacedAt = placedAt,
            Currency = currency,
        };

        foreach (var line in lines)
        {
            order._items.Add(OrderItem.Create(
                line.ProductId,
                line.ProductName,
                line.UnitPrice,
                line.Quantity));
        }

        InvariantViolationException.ThrowIf(
            order._items.Count == 0,
            "order.empty",
            "Пустой заказ оформить нельзя.");

        order.Recalculate(discountAmount);
        order.Raise(new OrderPlacedEvent(order.Id, order.UserId, order.Number, order.Total, order.Currency));

        return order;
    }

    public void MarkPaid(DateTimeOffset paidAt)
    {
        InvariantViolationException.ThrowIf(
            Status != OrderStatus.Pending,
            "order.cannot_pay",
            $"Оплатить можно только заказ в ожидании оплаты, текущий статус — {Status}.");

        Status = OrderStatus.Paid;
        PaidAt = paidAt;
    }

    public void Ship()
    {
        InvariantViolationException.ThrowIf(
            Status != OrderStatus.Paid,
            "order.cannot_ship",
            $"Отправить можно только оплаченный заказ, текущий статус — {Status}.");

        Status = OrderStatus.Shipped;
    }

    public void Complete()
    {
        InvariantViolationException.ThrowIf(
            Status != OrderStatus.Shipped,
            "order.cannot_complete",
            $"Завершить можно только отправленный заказ, текущий статус — {Status}.");

        Status = OrderStatus.Completed;
    }

    /// <summary>
    /// Отмена возможна, пока заказ не уехал к покупателю. Отправленный и завершённый
    /// заказ отменяется только возвратом, которого в проекте нет (docs/SPEC.md §1).
    /// </summary>
    public void Cancel(DateTimeOffset canceledAt)
    {
        InvariantViolationException.ThrowIf(
            Status is not (OrderStatus.Pending or OrderStatus.Paid),
            "order.cannot_cancel",
            $"Отменить можно только заказ до отправки, текущий статус — {Status}.");

        Status = OrderStatus.Canceled;
        CanceledAt = canceledAt;
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        InvariantViolationException.ThrowIf(
            Status != OrderStatus.Pending,
            "order.cannot_change",
            $"Менять сумму можно только у неоплаченного заказа, текущий статус — {Status}.");

        Recalculate(discountAmount);
    }

    private void Recalculate(decimal discountAmount)
    {
        InvariantViolationException.ThrowIf(
            discountAmount < 0m,
            "order.negative_discount",
            "Скидка не может быть отрицательной.");

        var subtotal = _items.Sum(item => item.LineTotal);

        InvariantViolationException.ThrowIf(
            discountAmount > subtotal,
            "order.discount_exceeds_subtotal",
            "Скидка не может превышать сумму заказа.");

        Subtotal = subtotal;
        DiscountAmount = discountAmount;
        Total = subtotal - discountAmount;
    }
}