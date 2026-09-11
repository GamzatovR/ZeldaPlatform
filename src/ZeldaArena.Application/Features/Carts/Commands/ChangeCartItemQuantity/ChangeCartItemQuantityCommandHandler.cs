using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts.Commands.ChangeCartItemQuantity;

public sealed class ChangeCartItemQuantityCommandHandler(
    CartLocator locator,
    IReadRepository<Product> products,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeCartItemQuantityCommand, Result<CartSummaryDto>>
{
    public async Task<Result<CartSummaryDto>> Handle(
        ChangeCartItemQuantityCommand request,
        CancellationToken cancellationToken)
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

        var product = await products.FindAsync(request.ProductId, cancellationToken).ConfigureAwait(false);

        if (CartStockCheck.Check(product, request.Quantity) is { } error)
        {
            return Result.Failure<CartSummaryDto>(error);
        }

        cart.ChangeQuantity(product!, request.Quantity);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new CartSummaryDto(cart.TotalQuantity));
    }
}