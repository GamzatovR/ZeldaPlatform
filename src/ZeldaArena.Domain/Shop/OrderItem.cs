using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.Shop;

/// <summary>Позиция заказа.</summary>
public class OrderItem : BaseEntity
{
    private OrderItem()
    {
    }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductNameSnapshot { get; private set; } = null!;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public Product? Product { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    internal static OrderItem Create(
        Guid productId,
        string productNameSnapshot,
        decimal unitPrice,
        int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productNameSnapshot);

        InvariantViolationException.ThrowIf(
            quantity < 1,
            "order.invalid_quantity",
            $"Количество товара начинается с единицы, получено {quantity}.");

        InvariantViolationException.ThrowIf(
            unitPrice < 0m,
            "order.negative_price",
            "Цена позиции не может быть отрицательной.");

        return new OrderItem
        {
            ProductId = productId,
            ProductNameSnapshot = productNameSnapshot.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity,
        };
    }
}