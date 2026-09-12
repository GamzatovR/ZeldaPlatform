namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductsForAdmin;

public sealed record AdminProductRowDto
{
    public Guid Id { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string Currency { get; init; } = string.Empty;

    public int StockQuantity { get; init; }

    public bool IsActive { get; init; }
}