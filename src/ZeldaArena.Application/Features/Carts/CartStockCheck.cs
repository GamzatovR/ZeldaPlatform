using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts;

/// <summary>
/// Можно ли держать в корзине столько штук товара. Те же условия проверяет
/// <c>Cart.AddItem</c>, но сущность отвечает исключением — последним рубежом, — а
/// пользователь должен увидеть сообщение: «На складе осталось 3 шт.» (docs/SPEC.md §15).
/// </summary>
public static class CartStockCheck
{
    public const int MaxQuantityPerLine = 99;

    public static Error? Check(Product? product, int wantedQuantity)
    {
        if (product is null)
        {
            return ShopErrors.ProductNotFound;
        }

        if (!product.IsActive)
        {
            return ShopErrors.ProductUnavailable;
        }

        if (product.StockQuantity < 1)
        {
            return ShopErrors.OutOfStock;
        }

        return wantedQuantity > product.StockQuantity
            ? ShopErrors.InsufficientStock(product.StockQuantity)
            : null;
    }
}