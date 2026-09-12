namespace ZeldaArena.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    Canceled = 3,

    /// <summary>
    /// Деньги возвращены покупателю: администратор отменил оплаченный заказ
    /// (docs/adr/ADR-0010). В выручку такой платёж не входит.
    /// </summary>
    Refunded = 4,
}