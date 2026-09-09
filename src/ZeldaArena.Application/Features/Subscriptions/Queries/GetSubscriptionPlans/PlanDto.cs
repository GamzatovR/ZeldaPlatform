namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;

/// <summary>
/// Тариф для страницы выбора (docs/SPEC.md §9.3, п. 17). Состав фич приходит списком
/// названий: пользователю показывают, что он получит, а не коды, которыми это
/// проверяется в коде.
/// </summary>
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