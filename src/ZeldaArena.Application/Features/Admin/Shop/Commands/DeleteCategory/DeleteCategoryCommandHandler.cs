using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(
    IRepository<ProductCategory> categories,
    IReadRepository<Product> products,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await categories.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(ShopErrors.CategoryNotFound);
        }

        if (await queryExecutor.AnyAsync(
                products.Query().Where(product => product.CategoryId == request.Id),
                cancellationToken))
        {
            return Result.Failure(ShopErrors.CategoryNotEmpty);
        }

        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}