namespace ZeldaArena.Domain.Billing;

/// <summary>
/// Привязка фичи к тарифу. Ключ составной (PlanId, FeatureId), поэтому BaseEntity не наследуется.
/// Именно эта таблица позволяет вынуть stats.advanced из тарифа Pro и продать её отдельно,
/// не трогая код (docs/SPEC.md §5.4, EP-4).
/// </summary>
public class PlanFeature
{
    private PlanFeature()
    {
    }

    public Guid PlanId { get; private set; }

    public Guid FeatureId { get; private set; }

    /// <summary>
    /// Параметр фичи: процент скидки, лимит команд и тому подобное.
    /// Позволяет менять поведение фичи из админки, без деплоя (EP-5).
    /// </summary>
    public string? Value { get; private set; }

    public Plan? Plan { get; private set; }

    public Feature? Feature { get; private set; }

    internal static PlanFeature Create(Guid planId, Guid featureId, string? value) =>
        new()
        {
            PlanId = planId,
            FeatureId = featureId,
            Value = string.IsNullOrWhiteSpace(value) ? null : value.Trim(),
        };

    internal void SetValue(string? value) =>
        Value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
