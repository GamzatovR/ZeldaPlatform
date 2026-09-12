namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;

public sealed record PlanDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int DurationDays,
    bool IsFree,
    IReadOnlyList<PlanFeatureDto> Features);