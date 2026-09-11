namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Письма биллинга (docs/SPEC.md §7.6): код подтверждения оплаты, чек, уведомление
/// об истечении подписки.
///
/// Порт объявлен здесь, а реализация живёт в Web — по той же причине, что и
/// <see cref="IAccountEmailSender"/> в Фазе 3 (docs/adr/ADR-0006): письмо это
/// локализованный текст и абсолютная ссылка, то есть ресурсы и таблица маршрутов.
/// Хендлер обязан сказать «отправь код», а не собирать HTML.
///
/// Код подтверждения передаётся сюда сырым и дальше письма не уходит.
/// </summary>
public interface IBillingEmailSender
{
    /// <summary>
    /// Письмо с кодом. Срок нужен в тексте: пользователь должен видеть, сколько
    /// у него времени, а не гадать (§15 — сообщения конкретны).
    /// </summary>
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

    /// <summary>Чек заказа магазина: номер, по которому заказ найдётся в истории, и сумма (§7.6, шаг 4).</summary>
    Task SendOrderReceiptAsync(
        string email,
        string? displayName,
        string orderNumber,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    /// <summary>Подписка закончилась — шлёт фоновая служба (§7.5, п. 4).</summary>
    Task SendSubscriptionExpiredAsync(
        string email,
        string? displayName,
        string planName,
        CancellationToken cancellationToken = default);
}