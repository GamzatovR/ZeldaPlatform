using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts.Commands.AddCartItem;

/// <summary>
/// Повторное добавление того же товара увеличивает количество позиции, поэтому остаток
/// проверяется по сумме «уже в корзине + добавляемое», а не по одному добавлению.
/// </summary>
public sealed class AddCartItemCommandHandler(
    CartLocator locator,
    IReadRepository<Product> products,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddCartItemCommand, Result<CartSummaryDto>>
{
    public async Task<Result<CartSummaryDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (locator.CurrentOwner() is not { } owner)
        {
            return Result.Failure<CartSummaryDto>(ShopErrors.CartUnavailable);
        }

        var product = await products.FindAsync(request.ProductId, cancellationToken).ConfigureAwait(false);
        var cart = await locator.FindAsync(owner, cancellationToken).ConfigureAwait(false);

        var alreadyInCart = cart?.Items.SingleOrDefault(item => item.ProductId == request.ProductId)?.Quantity ?? 0;

        if (CartStockCheck.Check(product, alreadyInCart + request.Quantity) is { } error)
        {
            return Result.Failure<CartSummaryDto>(error);
        }

        // Валидатор ограничивает одно добавление, но повторные добавления складываются.
        // Строку больше предела поле количества в корзине уже не смогло бы показать.
        if (alreadyInCart + request.Quantity > CartStockCheck.MaxQuantityPerLine)
        {
            return Result.Failure<CartSummaryDto>(ShopErrors.LineLimit(CartStockCheck.MaxQuantityPerLine));
        }

        cart ??= await locator.GetOrCreateAsync(owner, cancellationToken).ConfigureAwait(false);
        cart.AddItem(product!, request.Quantity);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new CartSummaryDto(cart.TotalQuantity));
    }
}