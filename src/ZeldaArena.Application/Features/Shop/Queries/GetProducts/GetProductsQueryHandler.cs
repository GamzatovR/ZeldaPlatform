using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Shop.Queries.GetProducts;

/// <summary>
/// Только товары в продаже: снятый с продажи товар не показывается, в корзину его
/// тоже не положить (<c>Cart.AddItem</c>).
///
/// Название категории — коррелированный подзапрос, а не навигация: EF Core переводит
/// его в скалярный подзапрос той же выборки (§16, без N+1), а в памяти он исполняется
/// так же, как в SQL, — навигация в тестах была бы пустой.
/// </summary>
public sealed class GetProductsQueryHandler(
    IReadRepository<Product> products,
    IReadRepository<ProductCategory> categories,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    public Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = products.Query().Where(product => product.IsActive);
        var allCategories = categories.Query();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            // Неизвестный слаг — пустой список, а не молча снятый фильтр.
            Slug.TryFrom(request.Category, out var categorySlug);

            query = query.Where(product => allCategories.Any(category =>
                category.Id == product.CategoryId && category.Slug == categorySlug));
        }

        if (request.PriceMin is { } priceMin)
        {
            query = query.Where(product => product.Price.Amount >= priceMin);
        }

        if (request.PriceMax is { } priceMax)
        {
            query = query.Where(product => product.Price.Amount <= priceMax);
        }

        if (request.InStock == true)
        {
            query = query.Where(product => product.StockQuantity > 0);
        }

        var projected = ProductSorting.Map
            .Apply(query, request.Sort)
            .Select(ProductProjection.ToListItem(allCategories));

        return queryExecutor.ToPagedResultAsync(
            projected,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}