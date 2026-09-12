using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetMySubscription;

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