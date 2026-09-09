namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;

/// <summary>
/// Фича в составе тарифа. Код нужен странице, чтобы подсветить тариф, которым
/// открывается функция из параметра <c>?required=</c> (docs/SPEC.md §7.3).
/// </summary>
public sealed record PlanFeatureDto(string Code, string Name, string? Description);