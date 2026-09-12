using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Admin.Shop.Queries.GetCategoriesForAdmin;
using ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductsForAdmin;

namespace ZeldaArena.Web.Areas.Admin.Models.Shop;

public sealed class ProductIndexViewModel
{
    public required GetProductsForAdminQuery Filter { get; init; }

    public required PagedResult<AdminProductRowDto> Result { get; init; }

    /// <summary>Категории для фильтра; в ответе API не нужны — там только таблица.</summary>
    public IReadOnlyList<AdminCategoryDto> Categories { get; init; } = [];

    public string Sort => AdminProductSorting.Map.Resolve(Filter.Sort);
}