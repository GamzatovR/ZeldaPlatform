namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlansForAdmin;

/// <summary>
/// Тариф для админки: в отличие от витрины показывает и снятые с продажи,
/// и число действующих подписок — по нему видно, кого затронет правка состава фич.
/// </summary>
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