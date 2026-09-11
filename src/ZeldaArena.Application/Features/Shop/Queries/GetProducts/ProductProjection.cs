using System.Linq.Expressions;

using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Shop.Queries.GetProducts;

/// <summary>
/// Одна проекция карточки на каталог и витрину главной: одна разметка
/// <c>ProductCard</c> — одни и те же поля.
/// </summary>
public static class ProductProjection
{
    public static Expression<Func<Product, ProductListItemDto>> ToListItem(IQueryable<ProductCategory> categories) =>
        product => new ProductListItemDto
        {
            Id = product.Id,
            Slug = product.Slug.Value,
            Name = product.Name,
            CategoryName = categories
                .Where(category => category.Id == product.CategoryId)
                .Select(category => category.Name)
                .FirstOrDefault() ?? string.Empty,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            StockQuantity = product.StockQuantity,
            ImagePath = product.ImagePath,
        };
}