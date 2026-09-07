namespace ZeldaArena.Domain.Shop;

/// <summary>
/// Строка будущего заказа: то, что хендлер оформления собрал из корзины,
/// подставив серверные цену и название товара.
/// </summary>
public readonly record struct OrderLine(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);