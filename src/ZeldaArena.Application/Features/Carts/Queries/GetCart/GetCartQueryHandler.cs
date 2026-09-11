using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts.Queries.GetCart;

/// <summary>
/// Позиции и товары — одним запросом с JOIN, корзина владельца — подзапросом
/// (docs/SPEC.md §16, без N+1). Сортировка по названию, а не по времени добавления:
/// строка не должна прыгать по таблице после изменения количества.
/// </summary>
public sealed class GetCartQueryHandler(
    CartLocator locator,
    IReadRepository<CartItem> cartItems,
    IReadRepository<Product> products,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        if (locator.CurrentOwner() is not { } owner)
        {
            return CartDto.Empty;
        }

        var owned = locator.OwnedBy(owner);

        var query =
            from item in cartItems.Query()
            join product in products.Query() on item.ProductId equals product.Id
            where owned.Any(cart => cart.Id == item.CartId)
            orderby product.Name
            select new CartLineDto
            {
                ProductId = product.Id,
                Slug = product.Slug.Value,
                Name = product.Name,
                ImagePath = product.ImagePath,
                UnitPrice = product.Price.Amount,
                Currency = product.Price.Currency,
                Quantity = item.Quantity,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
            };

        var lines = await queryExecutor.ToListAsync(query, cancellationToken).ConfigureAwait(false);

        return new CartDto { Lines = lines };
    }
}