namespace ZeldaArena.Application.Features.Orders;

/// <summary>Правила заказа, общие для сценариев и форм (docs/adr/ADR-0009).</summary>
public static class OrderRules
{
    /// <summary>Длина столбца <c>Orders.Number</c>: номер длиннее — заведомо не номер.</summary>
    public const int MaxNumberLength = 30;

    /// <summary>
    /// Сколько ждать после истечения кода, прежде чем считать заказ брошенным.
    /// Истёкший код ещё можно заменить новым («Отправить код повторно»), и отменять
    /// заказ сразу по истечении значило бы отнять у покупателя эту возможность.
    /// </summary>
    public static readonly TimeSpan AbandonedAfterCodeExpiry = TimeSpan.FromMinutes(30);
}