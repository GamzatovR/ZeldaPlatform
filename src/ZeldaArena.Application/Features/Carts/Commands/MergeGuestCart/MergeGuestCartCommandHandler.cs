using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts.Commands.MergeGuestCart;

public sealed class MergeGuestCartCommandHandler(
    ICurrentUserService currentUser,
    IGuestCartIdentity guest,
    CartLocator locator,
    IRepository<Cart> carts,
    IRepository<Product> products,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MergeGuestCartCommand, Result<CartSummaryDto>>
{
    public async Task<Result<CartSummaryDto>> Handle(MergeGuestCartCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || guest.AnonymousId is not { } anonymousId)
        {
            return Result.Success(CartSummaryDto.Empty);
        }

        var guestCart = await locator
            .FindAsync(CartOwner.ForGuest(anonymousId), cancellationToken)
            .ConfigureAwait(false);

        if (guestCart is null)
        {
            return Result.Success(CartSummaryDto.Empty);
        }

        var summary = CartSummaryDto.Empty;

        if (!guestCart.IsEmpty)
        {
            var userCart = await locator
                .GetOrCreateAsync(CartOwner.ForUser(userId), cancellationToken)
                .ConfigureAwait(false);

            // Товары грузятся одним запросом, а не по одному на позицию.
            var productIds = guestCart.Items.Select(item => item.ProductId).ToArray();
            var loaded = await products.GetByIdsAsync(productIds, cancellationToken).ConfigureAwait(false);

            userCart.MergeFrom(guestCart, loaded.ToDictionary(product => product.Id));
            summary = new CartSummaryDto(userCart.TotalQuantity);
        }

        carts.Remove(guestCart);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(summary);
    }
}