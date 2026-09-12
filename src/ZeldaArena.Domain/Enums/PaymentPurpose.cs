namespace ZeldaArena.Domain.Enums;

/// <summary>За что платят. Один механизм оплаты обслуживает и подписки, и заказы.</summary>
public enum PaymentPurpose
{
    Subscription = 0,
    Order = 1,
}