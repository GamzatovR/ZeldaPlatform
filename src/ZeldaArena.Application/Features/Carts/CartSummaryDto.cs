namespace ZeldaArena.Application.Features.Carts;

/// <summary>
/// Что показывает мини-корзина в шапке: сколько штук лежит в корзине. Команды корзины
/// отдают его в ответ, чтобы AJAX обновлял счётчик без второго запроса.
/// </summary>
public sealed record CartSummaryDto(int ItemCount)
{
    public static CartSummaryDto Empty { get; } = new(0);
}