using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Features.Shop.Queries.GetProducts;

public sealed record GetProductsQuery : FilterBase, IQuery<PagedResult<ProductListItemDto>>
{
    /// <summary>Слаг категории. Неизвестный слаг даёт пустой список, а не снятый фильтр.</summary>
    public string? Category { get; init; }

    public decimal? PriceMin { get; init; }

    public decimal? PriceMax { get; init; }

    /// <summary>Только товары, которые можно положить в корзину прямо сейчас.</summary>
    public bool? InStock { get; init; }
}