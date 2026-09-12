namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetFeatures;

public sealed record FeatureDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    int PlanCount);