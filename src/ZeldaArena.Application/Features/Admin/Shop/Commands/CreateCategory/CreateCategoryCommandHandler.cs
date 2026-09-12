using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Common.Slugs;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    IRepository<ProductCategory> categoryRepository,
    IReadRepository<ProductCategory> categories,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private const string SlugFallback = "category";

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name.Trim();
        var normalized = name.ToLowerInvariant();

        if (await queryExecutor.AnyAsync(
                categories.Query().Where(category => category.Name.ToLower() == normalized),
                cancellationToken))
        {
            return Result.Failure<Guid>(ShopErrors.CategoryNameTaken);
        }

        var slug = await SlugGenerator.UniqueAsync(
            name,
            SlugFallback,
            (candidate, token) => queryExecutor.AnyAsync(categories.Query().Where(category => category.Slug == candidate), token),
            cancellationToken);

        var category = ProductCategory.Create(slug, name);

        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}