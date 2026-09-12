using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Shop.Queries.GetProducts;

/// <summary>Разрешённые сортировки каталога — whitelist.</summary>
public static class ProductSorting
{
    public const string Newest = "newest";
    public const string PriceAscending = "price_asc";
    public const string PriceDescending = "price_desc";
    public const string NameAscending = "name_asc";

    public static readonly SortMap<Product> Map = new SortMap<Product>()
        .Add(
            Newest,
            query => query.OrderByDescending(product => product.CreatedAt).ThenBy(product => product.Name),
            isDefault: true)
        .Add(
            PriceAscending,
            query => query.OrderBy(product => product.Price.Amount).ThenBy(product => product.Name))
        .Add(
            PriceDescending,
            query => query.OrderByDescending(product => product.Price.Amount).ThenBy(product => product.Name))
        .Add(NameAscending, query => query.OrderBy(product => product.Name));
}