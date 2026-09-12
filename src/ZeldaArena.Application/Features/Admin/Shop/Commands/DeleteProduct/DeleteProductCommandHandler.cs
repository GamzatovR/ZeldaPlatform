using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IRepository<Product> products,
    IReadRepository<Order> orders,
    IQueryExecutor queryExecutor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = await products.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ShopErrors.ProductNotFound);
        }

        if (await queryExecutor.AnyAsync(
                orders.Query().SelectMany(order => order.Items).Where(item => item.ProductId == request.Id),
                cancellationToken))
        {
            return Result.Failure(ShopErrors.ProductHasOrders);
        }

        var image = product.ImagePath;

        products.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Фотографии сида лежат в wwwroot и принадлежат макету; удаляем только
        // то, что загрузили через админку (LocalFileStorage узнаёт свои имена).
        if (image is not null)
        {
            await storage.DeleteAsync(image, cancellationToken);
        }

        return Result.Success();
    }
}