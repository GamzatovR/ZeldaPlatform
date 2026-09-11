using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Models.Billing;

/// <summary>
/// Ошибки сценариев подписки и оплаты. Собраны в одном месте по образцу
/// <c>AccountErrors</c>: коды служат ключами ресурсов (docs/SPEC.md §9.5), поэтому
/// незакрытый перевод виден как код, а не как пустая строка.
///
/// Часть кодов совпадает с теми, что уже возвращает домен (<c>payment.code_expired</c>,
/// <c>payment.no_attempts_left</c>) — это намеренно: у одного исхода один ключ,
/// независимо от того, кто его обнаружил.
/// </summary>
public static class BillingErrors
{
    public static readonly Error PlanNotFound =
        new("billing.plan_not_found", "Тариф не найден.");

    public static readonly Error PlanInactive =
        new("billing.plan_inactive", "Тариф больше не продаётся.");

    /// <summary>Бесплатный тариф нельзя купить: платить не за что, а срока у него нет.</summary>
    public static readonly Error PlanNotPurchasable =
        new("billing.plan_not_purchasable", "Этот тариф нельзя оформить.");

    public static readonly Error PlanCodeTaken =
        new("billing.plan_code_taken", "Тариф с таким кодом уже существует.");

    public static readonly Error FeatureNotFound =
        new("billing.feature_not_found", "Платная функция не найдена.");

    public static readonly Error FeatureCodeTaken =
        new("billing.feature_code_taken", "Функция с таким кодом уже существует.");

    public static readonly Error PaymentNotFound =
        new("payment.not_found", "Платёж не найден.");

    public static readonly Error PaymentNotPending =
        new("payment.not_pending", "Этот платёж уже завершён.");

    public static readonly Error WrongCode =
        new("payment.wrong_code", "Код подтверждения неверен.");

    public static readonly Error CodeExpired =
        new("payment.code_expired", "Срок действия кода истёк.");

    public static readonly Error NoAttemptsLeft =
        new("payment.no_attempts_left", "Попытки ввода кода исчерпаны.");

    public static readonly Error AlreadyProcessed =
        new("payment.already_processed", "Платёж уже обработан.");

    /// <summary>
    /// Повторная отправка кода ограничена минутой (§7.6, шаг 3). Ограничение
    /// на уровне сценария, а не только rate limiting: иначе его обошёл бы
    /// второй браузер того же пользователя.
    /// </summary>
    public static readonly Error ResendTooSoon =
        new("payment.resend_too_soon", "Новый код можно запросить не чаще раза в минуту.");

    /// <summary>
    /// Единственный ответ на любую негодную карту. Что именно не так — номер, срок
    /// или CVV — знает валидатор формы; отказ авторизации подробностей не раскрывает.
    /// </summary>
    public static readonly Error CardDeclined =
        new("payment.card_declined", "Карта отклонена.");

    /// <summary>
    /// Платёж или то, что он оплачивает, изменили параллельно — вторая вкладка, отмена
    /// заказа, фоновая служба (docs/SPEC.md §15: конфликт конкурентности). Ничего не
    /// сохранено, код не израсходован: достаточно обновить страницу и повторить.
    /// </summary>
    public static readonly Error ConcurrentChange =
        new("payment.concurrent_change", "Платёж одновременно изменён в другой вкладке.");

    public static readonly Error SubscriptionNotFound =
        new("subscription.not_found", "Подписка не найдена.");

    public static readonly Error SubscriptionNotActive =
        new("subscription.not_active", "Активной подписки нет.");
}