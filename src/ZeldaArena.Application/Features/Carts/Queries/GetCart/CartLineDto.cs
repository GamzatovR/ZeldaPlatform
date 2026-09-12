namespace ZeldaArena.Application.Features.Carts.Queries.GetCart;

public sealed record CartLineDto
{
    public Guid ProductId { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? ImagePath { get; init; }

    public decimal UnitPrice { get; init; }

    public string Currency { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public int StockQuantity { get; init; }

    public bool IsActive { get; init; }

    public decimal LineTotal => UnitPrice * Quantity;

    public CartLineProblem Problem =>
        !IsActive ? CartLineProblem.Unavailable
        : StockQuantity < 1 ? CartLineProblem.OutOfStock
        : Quantity > StockQuantity ? CartLineProblem.InsufficientStock
        : CartLineProblem.None;

    /// <summary>Сколько можно выбрать в поле количества: не больше остатка и не больше предела позиции.</summary>
    public int MaxQuantity => Math.Clamp(StockQuantity, 1, CartStockCheck.MaxQuantityPerLine);
}