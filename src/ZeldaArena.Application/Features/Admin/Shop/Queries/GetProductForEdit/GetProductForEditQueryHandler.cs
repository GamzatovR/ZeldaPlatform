using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductForEdit;

public sealed class GetProductForEditQueryHandler(
    IReadRepository<Product> products,
    IReadRepository<Order> orders,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetProductForEditQuery, ProductEditDto?>
{
    public Task<ProductEditDto?> Handle(GetProductForEditQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = request.Id;

        var query = products.Query()
            .Where(product => product.Id == id)
            .Select(product => new ProductEditDto
            {
                Id = product.Id,
                Slug = product.Slug.Value,
                Sku = product.Sku,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Price = product.Price.Amount,
                Currency = product.Price.Currency,
                StockQuantity = product.StockQuantity,
                Description = product.Description,
                ImagePath = product.ImagePath,
                IsActive = product.IsActive,
                OrderCount = orders.Query().SelectMany(order => order.Items).Count(item => item.ProductId == id),
            });

        return queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
    }
}