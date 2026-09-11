namespace ZeldaArena.Application.Features.Shop.Queries.GetProducts;

/// <summary>Карточка товара в каталоге и на витрине главной.</summary>
public sealed record ProductListItemDto
{
    /// <summary>С какого остатка карточка предупреждает «осталось N шт.».</summary>
    public const int LowStockThreshold = 5;

    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string Currency { get; init; } = string.Empty;

    public int StockQuantity { get; init; }

    /// <summary>Статический путь от корня сайта или имя файла из хранилища.</summary>
    public string? ImagePath { get; init; }

    public bool IsInStock => StockQuantity > 0;

    public bool IsLowStock => StockQuantity is > 0 and <= LowStockThreshold;
}