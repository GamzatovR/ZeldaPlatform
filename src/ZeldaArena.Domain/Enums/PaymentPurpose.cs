namespace ZeldaArena.Domain.Enums;

/// <summary>За что платят. Один механизм оплаты обслуживает и подписки, и заказы (docs/SPEC.md §7.6).</summary>
public enum PaymentPurpose
{
    Subscription = 0,
    Order = 1,
}