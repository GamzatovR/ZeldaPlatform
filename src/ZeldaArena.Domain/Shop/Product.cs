using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Shop;

/// <summary>Товар магазина игровой периферии.</summary>
public class Product : BaseEntity, IAuditableEntity
{
    private Product()
    {
    }

    public Slug Slug { get; private set; } = null!;

    /// <summary>Артикул: уникален, ищется по нему в админке.</summary>
    public string Sku { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid CategoryId { get; private set; }

    public Money Price { get; private set; } = null!;

    public int StockQuantity { get; private set; }

    public string? ImagePath { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public ProductCategory? Category { get; private set; }

    public bool IsInStock => StockQuantity > 0;

    public static Product Create(
        Slug slug,
        string sku,
        string name,
        Guid categoryId,
        Money price,
        int stockQuantity,
        string? description = null,
        string? imagePath = null,
        bool isActive = true)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(price);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        InvariantViolationException.ThrowIf(
            categoryId == Guid.Empty,
            "product.category_required",
            "У товара обязана быть категория.");

        return new Product
        {
            Slug = slug,
            Sku = RequireSku(sku),
            Name = name.Trim(),
            CategoryId = categoryId,
            Price = price,
            StockQuantity = RequireStock(stockQuantity),
            Description = Normalize(description),
            ImagePath = Normalize(imagePath),
            IsActive = isActive,
        };
    }

    public void UpdateDetails(string name, Guid categoryId, string? description, string? imagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        InvariantViolationException.ThrowIf(
            categoryId == Guid.Empty,
            "product.category_required",
            "У товара обязана быть категория.");

        Name = name.Trim();
        CategoryId = categoryId;
        Description = Normalize(description);
        ImagePath = Normalize(imagePath);
    }

    public void ChangePrice(Money price)
    {
        ArgumentNullException.ThrowIfNull(price);
        Price = price;
    }

    public void SetStock(int stockQuantity) => StockQuantity = RequireStock(stockQuantity);

    /// <summary>Списывает остаток при оформлении заказа.</summary>
    public void DecreaseStock(int quantity)
    {
        InvariantViolationException.ThrowIf(
            quantity < 1,
            "product.invalid_quantity",
            $"Списываемое количество начинается с единицы, получено {quantity}.");

        InvariantViolationException.ThrowIf(
            quantity > StockQuantity,
            "product.insufficient_stock",
            $"На складе осталось {StockQuantity} шт.");

        StockQuantity -= quantity;
    }

    /// <summary>Возврат остатка при отмене заказа.</summary>
    public void IncreaseStock(int quantity)
    {
        InvariantViolationException.ThrowIf(
            quantity < 1,
            "product.invalid_quantity",
            $"Возвращаемое количество начинается с единицы, получено {quantity}.");

        StockQuantity += quantity;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string RequireSku(string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        return sku.Trim().ToUpperInvariant();
    }

    private static int RequireStock(int stockQuantity)
    {
        InvariantViolationException.ThrowIf(
            stockQuantity < 0,
            "product.negative_stock",
            "Остаток на складе не может быть отрицательным.");

        return stockQuantity;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}