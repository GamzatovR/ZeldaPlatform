using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetMySubscription;

/// <summary>
/// Текущая подписка пользователя для личного кабинета (docs/SPEC.md §9.3, п. 17).
///
/// Набор фич отдаётся кодами: по ним страница решает, что показывать, и по ним же
/// подсвечивается функция, которой пользователю не хватило (§7.3).
/// </summary>
public sealed record MySubscriptionDto(
    Guid Id,
    string PlanCode,
    string PlanName,
    SubscriptionStatus Status,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool AutoRenew,
    decimal PriceSnapshot,
    bool IsActive,
    IReadOnlyCollection<string> Features);