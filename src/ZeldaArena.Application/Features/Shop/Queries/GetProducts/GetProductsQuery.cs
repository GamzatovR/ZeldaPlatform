using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Features.Shop.Queries.GetProducts;

/// <summary>
/// Каталог магазина (docs/SPEC.md §9.3, п. 12): категория, цена от–до, наличие,
/// сортировка, пагинация. Поля повторяют query-string из §10.2:
/// <c>/shop?category=keyboards&amp;priceMin=3000&amp;priceMax=15000&amp;inStock=true&amp;sort=price_asc&amp;page=3&amp;pageSize=24</c>.
/// </summary>
public sealed record GetProductsQuery : FilterBase, IQuery<PagedResult<ProductListItemDto>>
{
    /// <summary>Слаг категории. Неизвестный слаг даёт пустой список, а не снятый фильтр.</summary>
    public string? Category { get; init; }

    public decimal? PriceMin { get; init; }

    public decimal? PriceMax { get; init; }

    /// <summary>Только товары, которые можно положить в корзину прямо сейчас.</summary>
    public bool? InStock { get; init; }
}