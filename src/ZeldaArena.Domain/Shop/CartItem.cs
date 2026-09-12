using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.Shop;

/// <summary>Позиция корзины.</summary>
public class CartItem : BaseEntity
{
    private CartItem()
    {
    }

    public Guid CartId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public decimal PriceSnapshot { get; private set; }

    public Product? Product { get; private set; }

    public decimal LineTotal => PriceSnapshot * Quantity;

    internal static CartItem Create(Guid cartId, Guid productId, int quantity, decimal priceSnapshot)
    {
        InvariantViolationException.ThrowIf(
            quantity < 1,
            "cart.invalid_quantity",
            $"Количество товара начинается с единицы, получено {quantity}.");

        return new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity,
            PriceSnapshot = priceSnapshot,
        };
    }

    internal void SetQuantity(int quantity)
    {
        InvariantViolationException.ThrowIf(
            quantity < 1,
            "cart.invalid_quantity",
            $"Количество товара начинается с единицы, получено {quantity}.");

        Quantity = quantity;
    }

    internal void RefreshPrice(decimal priceSnapshot) => PriceSnapshot = priceSnapshot;
}