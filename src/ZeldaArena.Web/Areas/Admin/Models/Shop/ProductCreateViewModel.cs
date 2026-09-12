using ZeldaArena.Application.Features.Admin.Shop.Queries.GetCategoriesForAdmin;

namespace ZeldaArena.Web.Areas.Admin.Models.Shop;

public sealed class ProductCreateViewModel
{
    public required ProductFormViewModel Form { get; init; }

    public required IReadOnlyList<AdminCategoryDto> Categories { get; init; }
}