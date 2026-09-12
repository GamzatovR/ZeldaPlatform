using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductsForAdmin;

public static class AdminProductSorting
{
    public const string NameAscending = "name_asc";
    public const string NameDescending = "name_desc";
    public const string PriceAscending = "price_asc";
    public const string PriceDescending = "price_desc";
    public const string StockAscending = "stock_asc";
    public const string StockDescending = "stock_desc";

    public static readonly SortMap<Product> Map = new SortMap<Product>()
        .Add(NameAscending, query => query.OrderBy(product => product.Name), isDefault: true)
        .Add(NameDescending, query => query.OrderByDescending(product => product.Name))
        .Add(PriceAscending, query => query.OrderBy(product => product.Price.Amount))
        .Add(PriceDescending, query => query.OrderByDescending(product => product.Price.Amount))
        .Add(StockAscending, query => query.OrderBy(product => product.StockQuantity))
        .Add(StockDescending, query => query.OrderByDescending(product => product.StockQuantity));
}