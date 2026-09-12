using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Application.Common.Slugs;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IRepository<Product> productRepository,
    IReadRepository<Product> products,
    IReadRepository<ProductCategory> categories,
    IQueryExecutor queryExecutor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private const string SlugFallback = "product";

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await categories.FindAsync(request.CategoryId, cancellationToken) is null)
        {
            return Result.Failure<Guid>(ShopErrors.CategoryNotFound);
        }

        var sku = request.Sku.Trim();

        if (await queryExecutor.AnyAsync(products.Query().Where(product => product.Sku == sku), cancellationToken))
        {
            return Result.Failure<Guid>(ShopErrors.SkuTaken);
        }

        if (!await StoredImages.IsAcceptableAsync(request.Image, cancellationToken))
        {
            return Result.Failure<Guid>(FileErrors.InvalidImage);
        }

        var slug = await SlugGenerator.UniqueAsync(
            request.Name,
            SlugFallback,
            (candidate, token) => queryExecutor.AnyAsync(products.Query().Where(product => product.Slug == candidate), token),
            cancellationToken);

        Product? product = null;

        var created = DomainRules.Apply(() => product = Product.Create(
            slug,
            sku,
            request.Name,
            request.CategoryId,
            Money.FromRubles(request.Price),
            request.StockQuantity,
            request.Description));

        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        await StoredImages.ReplaceAsync(
            storage,
            request.Image,
            remove: false,
            previous: null,
            apply: path => product!.UpdateDetails(product.Name, product.CategoryId, product.Description, path),
            persist: async () =>
            {
                await productRepository.AddAsync(product!, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);

        return product!.Id;
    }
}