namespace ZeldaArena.Application.Features.Admin.Billing.Commands.SetPlanFeatures;

/// <summary>
/// Фича в составе тарифа и её параметр (docs/SPEC.md §5.4, EP-5): лимит команд,
/// процент скидки и тому подобное. Пустое значение означает обычную фичу,
/// у которой важен сам факт наличия.
/// </summary>
public sealed record PlanFeatureAssignment(Guid FeatureId, string? Value);