using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.RenameCategory;

public sealed class RenameCategoryCommandHandler(
    IRepository<ProductCategory> categoryRepository,
    IReadRepository<ProductCategory> categories,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RenameCategoryCommand, Result>
{
    public async Task<Result> Handle(RenameCategoryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(ShopErrors.CategoryNotFound);
        }

        var normalized = request.Name.Trim().ToLowerInvariant();

        if (await queryExecutor.AnyAsync(
                categories.Query().Where(other => other.Id != request.Id && other.Name.ToLower() == normalized),
                cancellationToken))
        {
            return Result.Failure(ShopErrors.CategoryNameTaken);
        }

        var result = DomainRules.Apply(() => category.Rename(request.Name));

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}