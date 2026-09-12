namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;

/// <summary>
/// Фича в списке тарифа. <paramref name="Value"/> — параметр фичи (EP-5): например,
/// сколько команд разрешено создавать.
/// </summary>
public sealed record PlanFeatureOptionDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    bool IsGranted,
    string? Value);