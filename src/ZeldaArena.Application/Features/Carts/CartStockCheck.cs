using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts;

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