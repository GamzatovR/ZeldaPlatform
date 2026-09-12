namespace ZeldaArena.Application.Features.Orders;

/// <summary>Правила заказа, общие для сценариев и форм (docs/adr/ADR-0009).</summary>
public static class OrderRules
{
    /// <summary>Длина столбца <c>Orders.Number</c>: номер длиннее — заведомо не номер.</summary>
    public const int MaxNumberLength = 30;

    public static readonly TimeSpan AbandonedAfterCodeExpiry = TimeSpan.FromMinutes(30);
}