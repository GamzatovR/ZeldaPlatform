namespace ZeldaArena.Application.Common.Interfaces;

public interface IBillingEmailSender
{
    Task SendPaymentCodeAsync(
        string email,
        string? displayName,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default);

    /// <summary>Чек после успешного подтверждения: что оплачено, на какую сумму и до какого числа.</summary>
    Task SendPaymentReceiptAsync(
        string email,
        string? displayName,
        string planName,
        decimal amount,
        string currency,
        DateTimeOffset subscriptionEndsAt,
        CancellationToken cancellationToken = default);

    /// <summary>Чек заказа магазина: номер, по которому заказ найдётся в истории, и сумма.</summary>
    Task SendOrderReceiptAsync(
        string email,
        string? displayName,
        string orderNumber,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    /// <summary>Подписка закончилась — шлёт фоновая служба.</summary>
    Task SendSubscriptionExpiredAsync(
        string email,
        string? displayName,
        string planName,
        CancellationToken cancellationToken = default);
}