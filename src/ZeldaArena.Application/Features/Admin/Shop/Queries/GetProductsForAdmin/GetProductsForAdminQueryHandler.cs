using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductsForAdmin;

public sealed class GetProductsForAdminQueryHandler(
    IReadRepository<Product> products,
    IReadRepository<ProductCategory> categories,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetProductsForAdminQuery, PagedResult<AdminProductRowDto>>
{
    public Task<PagedResult<AdminProductRowDto>> Handle(
        GetProductsForAdminQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = products.Query();

        if (request.CategoryId is { } categoryId)
        {
            query = query.Where(product => product.CategoryId == categoryId);
        }

        if (request.IsActive is { } isActive)
        {
            query = query.Where(product => product.IsActive == isActive);
        }

        if (request.OutOfStock is { } outOfStock)
        {
            query = outOfStock
                ? query.Where(product => product.StockQuantity == 0)
                : query.Where(product => product.StockQuantity > 0);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = request.Search.Trim().ToLowerInvariant();

            query = query.Where(product => product.Name.ToLower().Contains(pattern)
                || product.Sku.ToLower().Contains(pattern));
        }

        var rows = AdminProductSorting.Map
            .Apply(query, request.Sort)
            .Select(product => new AdminProductRowDto
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                CategoryName = categories.Query()
                    .Where(category => category.Id == product.CategoryId)
                    .Select(category => category.Name)
                    .FirstOrDefault() ?? string.Empty,
                Price = product.Price.Amount,
                Currency = product.Price.Currency,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
            });

        return queryExecutor.ToPagedResultAsync(
            rows,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}