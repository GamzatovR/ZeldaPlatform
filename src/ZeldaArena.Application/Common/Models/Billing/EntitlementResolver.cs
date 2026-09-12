using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Application.Common.Models.Billing;

public static class EntitlementResolver
{
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

        // Чей параметр победит при пересечении двух подписок, решает срок.
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
                // Отключённая фича прав не даёт, даже если осталась привязанной к тарифу.
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