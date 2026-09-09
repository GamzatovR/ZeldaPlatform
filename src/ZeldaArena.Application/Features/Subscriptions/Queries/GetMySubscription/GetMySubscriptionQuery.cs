using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetMySubscription;

/// <summary>
/// Подписка текущего пользователя. Возвращает <c>null</c>, если её нет, —
/// это не ошибка, а обычное состояние большинства посетителей.
/// </summary>
public sealed record GetMySubscriptionQuery : IQuery<MySubscriptionDto?>;