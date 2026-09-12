using ZeldaArena.Application.Features.Admin.Shop.Queries.GetCategoriesForAdmin;
using ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductForEdit;

namespace ZeldaArena.Web.Areas.Admin.Models.Shop;

public sealed class ProductEditViewModel
{
    public required ProductEditDto Product { get; init; }

    public required ProductFormViewModel Form { get; init; }

    public required IReadOnlyList<AdminCategoryDto> Categories { get; init; }
}