using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetMySubscription;

/// <summary>
/// Отдаёт подписку текущего пользователя вместе с набором его прав.
///
/// Права берутся у <see cref="IEntitlementService"/>, а не собираются здесь заново
/// по тарифу: источник истины о доступе один (§7.3, §20 пункт 2). Разойдясь, эти два
/// пути показали бы в кабинете одно, а на деле дали другое.
///
/// Заявки в ожидании оплаты пропускаются: неоплаченная подписка — ещё не подписка.
/// </summary>
public sealed class GetMySubscriptionQueryHandler(
    ICurrentUserService currentUser,
    IReadRepository<Subscription> subscriptions,
    IQueryExecutor queryExecutor,
    IEntitlementService entitlements,
    IDateTimeProvider clock)
    : IRequestHandler<GetMySubscriptionQuery, MySubscriptionDto?>
{
    public async Task<MySubscriptionDto?> Handle(
        GetMySubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return null;
        }

        var now = clock.UtcNow;

        // Действующая подписка идёт первой; если её нет, показывается последняя
        // закончившаяся — пользователю важно видеть, что было, а не пустую страницу.
        var query = subscriptions.Query()
            .Where(subscription => subscription.UserId == userId
                && subscription.Status != SubscriptionStatus.Pending)
            .OrderByDescending(subscription =>
                subscription.Status == SubscriptionStatus.Active && subscription.EndsAt > now)
            .ThenByDescending(subscription => subscription.EndsAt)
            .Select(subscription => new
            {
                subscription.Id,
                PlanCode = subscription.Plan!.Code,
                PlanName = subscription.Plan.Name,
                subscription.Status,
                subscription.StartsAt,
                subscription.EndsAt,
                subscription.AutoRenew,
                subscription.PriceSnapshot,
            });

        var current = await queryExecutor.FirstOrDefaultAsync(query, cancellationToken)
            .ConfigureAwait(false);

        if (current is null)
        {
            return null;
        }

        var granted = await entitlements.GetEntitlementsAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        return new MySubscriptionDto(
            current.Id,
            current.PlanCode,
            current.PlanName,
            current.Status,
            current.StartsAt,
            current.EndsAt,
            current.AutoRenew,
            current.PriceSnapshot,
            current.Status == SubscriptionStatus.Active && current.EndsAt > now,
            [.. granted.Features.Keys]);
    }
}