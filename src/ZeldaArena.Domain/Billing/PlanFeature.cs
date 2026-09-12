namespace ZeldaArena.Domain.Billing;

/// <summary>Привязка фичи к тарифу.</summary>
public class PlanFeature
{
    private PlanFeature()
    {
    }

    public Guid PlanId { get; private set; }

    public Guid FeatureId { get; private set; }

    /// <summary>Параметр фичи.</summary>
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