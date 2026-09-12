using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts.Queries.GetMiniCart;

public sealed class GetMiniCartQueryHandler(
    CartLocator locator,
    IReadRepository<CartItem> cartItems,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetMiniCartQuery, CartSummaryDto>
{
    public async Task<CartSummaryDto> Handle(GetMiniCartQuery request, CancellationToken cancellationToken)
    {
        if (locator.CurrentOwner() is not { } owner)
        {
            return CartSummaryDto.Empty;
        }

        var owned = locator.OwnedBy(owner);

        var quantities = await queryExecutor
            .ToListAsync(
                cartItems.Query()
                    .Where(item => owned.Any(cart => cart.Id == item.CartId))
                    .Select(item => item.Quantity),
                cancellationToken)
            .ConfigureAwait(false);

        return new CartSummaryDto(quantities.Sum());
    }
}