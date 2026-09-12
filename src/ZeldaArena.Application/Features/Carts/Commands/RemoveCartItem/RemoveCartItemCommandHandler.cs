using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Carts.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler(
    CartLocator locator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveCartItemCommand, Result<CartSummaryDto>>
{
    public async Task<Result<CartSummaryDto>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (locator.CurrentOwner() is not { } owner)
        {
            return Result.Failure<CartSummaryDto>(ShopErrors.CartUnavailable);
        }

        var cart = await locator.FindAsync(owner, cancellationToken).ConfigureAwait(false);

        if (cart is null || cart.Items.All(item => item.ProductId != request.ProductId))
        {
            return Result.Failure<CartSummaryDto>(ShopErrors.ItemNotFound);
        }

        cart.RemoveItem(request.ProductId);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new CartSummaryDto(cart.TotalQuantity));
    }
}