using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetCategoriesForAdmin;

/// <summary>Категории с числом товаров: по нему видно, какую можно удалить.</summary>
public sealed record GetCategoriesForAdminQuery : IQuery<IReadOnlyList<AdminCategoryDto>>;