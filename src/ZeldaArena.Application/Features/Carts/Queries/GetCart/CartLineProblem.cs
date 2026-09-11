namespace ZeldaArena.Application.Features.Carts.Queries.GetCart;

/// <summary>
/// Почему позицию нельзя купить прямо сейчас. Корзина живёт долго, а остаток и статус
/// товара меняются без её участия, поэтому проблема вычисляется при каждом показе,
/// а не хранится (docs/SPEC.md §9.3, п. 13: «проверка остатков»).
/// </summary>
public enum CartLineProblem
{
    None = 0,
    Unavailable = 1,
    OutOfStock = 2,
    InsufficientStock = 3,
}