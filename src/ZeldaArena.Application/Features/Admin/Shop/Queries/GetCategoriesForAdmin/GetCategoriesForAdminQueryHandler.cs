using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetCategoriesForAdmin;

public sealed class GetCategoriesForAdminQueryHandler(
    IReadRepository<ProductCategory> categories,
    IReadRepository<Product> products,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetCategoriesForAdminQuery, IReadOnlyList<AdminCategoryDto>>
{
    public Task<IReadOnlyList<AdminCategoryDto>> Handle(
        GetCategoriesForAdminQuery request,
        CancellationToken cancellationToken) =>
        queryExecutor.ToListAsync(
            categories.Query()
                .OrderBy(category => category.Name)
                .Select(category => new AdminCategoryDto(
                    category.Id,
                    category.Slug.Value,
                    category.Name,
                    products.Query().Count(product => product.CategoryId == category.Id))),
            cancellationToken);
}