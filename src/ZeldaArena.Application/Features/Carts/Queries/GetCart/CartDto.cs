using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Carts.Queries.GetCart;

/// <summary>
/// Корзина для страницы <c>/cart</c> и оформления (docs/SPEC.md §9.3, п. 13–14).
/// Итог считается здесь, на сервере, из текущих цен; вьюха его только печатает.
/// </summary>
public sealed record CartDto
{
    public static CartDto Empty { get; } = new();

    public IReadOnlyList<CartLineDto> Lines { get; init; } = [];

    public string Currency { get; init; } = Money.DefaultCurrency;

    public bool IsEmpty => Lines.Count == 0;

    public int ItemCount => Lines.Sum(line => line.Quantity);

    public decimal Subtotal => Lines.Sum(line => line.LineTotal);

    /// <summary>Оформить можно только корзину, в которой каждая позиция покупается как есть.</summary>
    public bool CanCheckout => !IsEmpty && Lines.All(line => line.Problem == CartLineProblem.None);
}