using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetMySubscription;

public sealed record GetMySubscriptionQuery : IQuery<MySubscriptionDto?>;