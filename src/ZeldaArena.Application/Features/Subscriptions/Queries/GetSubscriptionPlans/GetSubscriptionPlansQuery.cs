using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;

/// <summary>
/// Тарифы, доступные к покупке, с составом фич (docs/SPEC.md §7.2).
///
/// Список не захардкожен и не перечисляет тарифы по кодам: он целиком приходит
/// из базы, поэтому четвёртый тариф, заведённый администратором, появляется
/// на странице сам (EP-3, EP-4).
/// </summary>
public sealed record GetSubscriptionPlansQuery : IQuery<IReadOnlyList<PlanDto>>;