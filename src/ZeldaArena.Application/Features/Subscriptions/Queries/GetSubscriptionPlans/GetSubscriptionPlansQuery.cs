using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;

public sealed record GetSubscriptionPlansQuery : IQuery<IReadOnlyList<PlanDto>>;