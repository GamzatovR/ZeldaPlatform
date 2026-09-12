using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Models.Billing;

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

    public static readonly Error PlanHasSubscriptions =
        new("plan.has_subscriptions", "У тарифа есть подписки — удалить его нельзя, можно снять с продажи.");

    public static readonly Error FeatureInUse =
        new("feature.in_use", "Функция входит в тариф — сначала уберите её из всех тарифов.");

    public static readonly Error FeatureReferencedByCode =
        new("feature.referenced_by_code", "На эту функцию ссылается код приложения — удалить её нельзя.");

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

    public static readonly Error ResendTooSoon =
        new("payment.resend_too_soon", "Новый код можно запросить не чаще раза в минуту.");

    public static readonly Error CardDeclined =
        new("payment.card_declined", "Карта отклонена.");

    public static readonly Error ConcurrentChange =
        new("payment.concurrent_change", "Платёж одновременно изменён в другой вкладке.");

    public static readonly Error SubscriptionNotFound =
        new("subscription.not_found", "Подписка не найдена.");

    public static readonly Error SubscriptionNotActive =
        new("subscription.not_active", "Активной подписки нет.");
}