namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;

public sealed record PlanFeatureOptionDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    bool IsGranted,
    string? Value);