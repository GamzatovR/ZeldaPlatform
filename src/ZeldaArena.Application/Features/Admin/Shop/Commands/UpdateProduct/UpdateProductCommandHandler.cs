using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.UpdateProduct;

/// <summary>
/// Остаток выставляется целиком (<c>SetStock</c>), а не прибавляется: администратор
/// вводит то, что видит на складе. Гонку с оформлением заказа ловит токен
/// конкурентности товара (xmin) — сохранение упадёт конфликтом, а не затрёт списание.
/// </summary>
public sealed class UpdateProductCommandHandler(
    IRepository<Product> products,
    IReadRepository<ProductCategory> categories,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = await products.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ShopErrors.ProductNotFound);
        }

        if (await categories.FindAsync(request.CategoryId, cancellationToken) is null)
        {
            return Result.Failure(ShopErrors.CategoryNotFound);
        }

        if (!await StoredImages.IsAcceptableAsync(request.Image, cancellationToken))
        {
            return Result.Failure(FileErrors.InvalidImage);
        }

        var updated = DomainRules.Apply(() =>
        {
            product.ChangePrice(Money.FromRubles(request.Price));
            product.SetStock(request.StockQuantity);

            if (request.IsActive)
            {
                product.Activate();
            }
            else
            {
                product.Deactivate();
            }
        });

        if (updated.IsFailure)
        {
            return updated;
        }

        await StoredImages.ReplaceAsync(
            storage,
            request.Image,
            request.RemoveImage,
            product.ImagePath,
            path => product.UpdateDetails(request.Name, request.CategoryId, request.Description, path),
            () => unitOfWork.SaveChangesAsync(cancellationToken),
            cancellationToken);

        return Result.Success();
    }
}