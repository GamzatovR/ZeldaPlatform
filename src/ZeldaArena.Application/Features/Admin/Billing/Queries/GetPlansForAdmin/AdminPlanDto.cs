namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlansForAdmin;

public sealed record AdminPlanDto(
    Guid Id,
    string Code,
    string Name,
    decimal Price,
    string Currency,
    int DurationDays,
    bool IsActive,
    int SortOrder,
    IReadOnlyList<Guid> FeatureIds,
    int ActiveSubscriptions);