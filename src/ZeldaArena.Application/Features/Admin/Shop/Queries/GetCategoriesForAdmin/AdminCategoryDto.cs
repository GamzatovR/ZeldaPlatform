namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetCategoriesForAdmin;

public sealed record AdminCategoryDto(Guid Id, string Slug, string Name, int ProductCount);