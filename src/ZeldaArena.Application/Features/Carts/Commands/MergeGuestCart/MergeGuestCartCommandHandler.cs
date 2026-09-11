using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts.Commands.MergeGuestCart;

/// <summary>
/// Количества складываются, но не выше остатка; товары, снятые с продажи или
/// закончившиеся, пропускаются — это правило <c>Cart.MergeFrom</c>. Гостевая корзина
/// после слияния удаляется: второй раз её содержимое в корзину пользователя не попадёт,
/// даже если кука почему-то не снимется.
///
/// Нечего сливать — не ошибка: гость мог ничего не класть в корзину.
/// </summary>
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

            // Товары грузятся одним запросом, а не по одному на позицию (§16).
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