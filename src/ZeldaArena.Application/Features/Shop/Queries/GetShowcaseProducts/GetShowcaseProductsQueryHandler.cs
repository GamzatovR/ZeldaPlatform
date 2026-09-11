using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Shop.Queries.GetShowcaseProducts;

public sealed class GetShowcaseProductsQueryHandler(
    IReadRepository<Product> products,
    IReadRepository<ProductCategory> categories,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetShowcaseProductsQuery, IReadOnlyList<ProductListItemDto>>
{
    public Task<IReadOnlyList<ProductListItemDto>> Handle(
        GetShowcaseProductsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = products.Query()
            .Where(product => product.IsActive && product.StockQuantity > 0)
            .OrderByDescending(product => product.CreatedAt)
            .ThenBy(product => product.Name)
            .Take(request.Count)
            .Select(ProductProjection.ToListItem(categories.Query()));

        return queryExecutor.ToListAsync(query, cancellationToken);
    }
}