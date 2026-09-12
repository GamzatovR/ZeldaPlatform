namespace ZeldaArena.Domain.Shop;

/// <summary>Строка будущего заказа.</summary>
public readonly record struct OrderLine(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);