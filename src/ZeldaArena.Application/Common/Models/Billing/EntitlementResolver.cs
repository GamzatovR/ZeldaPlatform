using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Application.Common.Models.Billing;

/// <summary>
/// Собирает набор прав пользователя из его подписок (docs/SPEC.md §7.3).
///
/// Чистая функция без обращений к базе — и это осознанный выбор места. Реализация
/// <c>IEntitlementService</c> живёт в Infrastructure (§5.3), а <c>ZeldaArena.UnitTests</c>
/// по §5.1 ссылается только на Domain и Application, то есть напрямую её не проверить.
/// Инфраструктуре остаётся загрузка и кэш, а правила отбора — здесь, где их покрывают
/// обычные unit-тесты.
///
/// Тарифы и фичи передаются словарями, а не читаются через навигационные свойства:
/// <c>PlanFeature.Feature</c> заполняет EF, и в тесте он был бы null. Объёмы мизерные —
/// тарифов и фич единицы, подписок у пользователя тем более.
/// </summary>
public static class EntitlementResolver
{
    /// <summary>
    /// Права = объединение фич по всем действующим подпискам. Тариф в результат
    /// не попадает: код спрашивает «есть ли фича», а не «какой тариф» (§7.1).
    /// </summary>
    public static EntitlementSet Resolve(
        IEnumerable<Subscription> subscriptions,
        IReadOnlyDictionary<Guid, Plan> plansById,
        IReadOnlyDictionary<Guid, Feature> featuresById,
        DateTimeOffset moment)
    {
        ArgumentNullException.ThrowIfNull(subscriptions);
        ArgumentNullException.ThrowIfNull(plansById);
        ArgumentNullException.ThrowIfNull(featuresById);

        var features = new Dictionary<string, string?>(StringComparer.Ordinal);

        // Чей параметр победит при пересечении двух подписок, решает срок: у гранта
        // из подписки, которая заканчивается позже, приоритет. Правило нужно ради EP-5,
        // где Value — это лимит или процент, и «какое-нибудь из двух» не ответ.
        var decidedBy = new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);

        foreach (var subscription in subscriptions)
        {
            // Единственный критерий действующей подписки объявлен в домене
            // и здесь не переписывается.
            if (!subscription.IsActiveAt(moment))
            {
                continue;
            }

            if (!plansById.TryGetValue(subscription.PlanId, out var plan))
            {
                continue;
            }

            foreach (var planFeature in plan.PlanFeatures)
            {
                // Отключённая фича прав не даёт, даже если осталась привязанной
                // к тарифу: выключатель в админке обязан действовать сразу (EP-3).
                // Активность самого тарифа при этом не проверяется — IsActive у плана
                // означает «продаётся», а не «действует»: снятие тарифа с продажи
                // не должно отбирать права у тех, кто уже заплатил.
                if (!featuresById.TryGetValue(planFeature.FeatureId, out var feature)
                    || !feature.IsActive)
                {
                    continue;
                }

                if (decidedBy.TryGetValue(feature.Code, out var decidedAt)
                    && decidedAt >= subscription.EndsAt)
                {
                    continue;
                }

                features[feature.Code] = planFeature.Value;
                decidedBy[feature.Code] = subscription.EndsAt;
            }
        }

        return features.Count == 0 ? EntitlementSet.Empty : new EntitlementSet(features);
    }
}