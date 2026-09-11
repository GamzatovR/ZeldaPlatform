using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Shop.Queries.GetShopFilterOptions;

public sealed class GetShopFilterOptionsQueryHandler(
    IReadRepository<ProductCategory> categories,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetShopFilterOptionsQuery, IReadOnlyList<CategoryOptionDto>>
{
    public Task<IReadOnlyList<CategoryOptionDto>> Handle(
        GetShopFilterOptionsQuery request,
        CancellationToken cancellationToken) =>
        queryExecutor.ToListAsync(
            categories.Query()
                .OrderBy(category => category.Name)
                .Select(category => new CategoryOptionDto(category.Slug.Value, category.Name)),
            cancellationToken);
}