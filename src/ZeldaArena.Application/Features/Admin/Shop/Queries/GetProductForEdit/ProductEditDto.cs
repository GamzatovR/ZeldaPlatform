namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductForEdit;

public sealed record ProductEditDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public Guid CategoryId { get; init; }

    public decimal Price { get; init; }

    public string Currency { get; init; } = string.Empty;

    public int StockQuantity { get; init; }

    public string? Description { get; init; }

    public string? ImagePath { get; init; }

    public bool IsActive { get; init; }

    /// <summary>Сколько раз товар попадал в заказы: по этому числу решается удаление.</summary>
    public int OrderCount { get; init; }

    /// <summary>Товар, который уже покупали, снимается с продажи, а не удаляется (ADR-0010).</summary>
    public bool CanDelete => OrderCount == 0;
}