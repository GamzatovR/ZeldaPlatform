using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Application.Features.Shop.Queries.GetShopFilterOptions;

namespace ZeldaArena.Web.Models.Shop;

/// <summary>Каталог (docs/SPEC.md §9.3, п. 12): фильтр из адреса, категории и страница результата.</summary>
public sealed class ShopListViewModel
{
    public required GetProductsQuery Filter { get; init; }

    public required IReadOnlyList<CategoryOptionDto> Categories { get; init; }

    public required PagedResult<ProductListItemDto> Result { get; init; }
}