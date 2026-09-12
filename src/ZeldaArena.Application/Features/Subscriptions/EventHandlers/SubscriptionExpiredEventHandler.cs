using MediatR;

using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Events;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.Application.Features.Subscriptions.EventHandlers;

/// <summary>
/// Подписка истекла или отозвана (docs/SPEC.md §7.5, п. 4): снимаем роль Premium
/// и сбрасываем кэш прав.
///
/// Роль снимается не безусловно. У пользователя может остаться вторая действующая
/// подписка — например, купленная отдельно фича из выделенного тарифа (EP-4), —
/// и тогда бейдж должен сохраниться. Проверка идёт по базе, а не по событию:
/// событие знает только о своей подписке.
///
/// Команда пользователя при этом остаётся в целости, но её редактирование
/// закрывается отсутствием фичи <c>team.create</c> — решение зафиксировано
/// в docs/CONVENTIONS.md, раздел «При неопределённости».
/// </summary>
public sealed class SubscriptionExpiredEventHandler(
    IUserAccountService userAccounts,
    IEntitlementCacheInvalidator entitlementCache,
    IReadRepository<Subscription> subscriptions,
    IQueryExecutor queryExecutor,
    IDateTimeProvider clock,
    ILogger<SubscriptionExpiredEventHandler> logger)
    : INotificationHandler<DomainEventNotification<SubscriptionExpiredEvent>>
{
    public async Task Handle(
        DomainEventNotification<SubscriptionExpiredEvent> notification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var userId = notification.DomainEvent.UserId;
        var expiredId = notification.DomainEvent.SubscriptionId;

        await entitlementCache.InvalidateUserAsync(userId, cancellationToken).ConfigureAwait(false);

        if (await HasAnotherActiveSubscriptionAsync(userId, expiredId, cancellationToken)
            .ConfigureAwait(false))
        {
            return;
        }

        var removed = await userAccounts
            .RemoveFromRoleAsync(userId, RoleNames.Premium, cancellationToken)
            .ConfigureAwait(false);

        // Оставшийся бейдж — косметическая беда: доступа он не даёт, права уже
        // пересчитаны сбросом кэша выше. Ронять фоновую службу из-за него нельзя,
        // иначе остальные истёкшие подписки так и не обработаются.
        if (removed.IsFailure)
        {
            logger.LogWarning(
                "Не удалось снять роль {Role} с пользователя {UserId} после истечения подписки: {Error}",
                RoleNames.Premium,
                userId,
                removed.Error.Code);
        }
    }

    private Task<bool> HasAnotherActiveSubscriptionAsync(
        Guid userId,
        Guid expiredId,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        var query = subscriptions.Query()
            .Where(subscription => subscription.UserId == userId
                && subscription.Id != expiredId
                && subscription.Status == SubscriptionStatus.Active
                && subscription.EndsAt > now);

        return queryExecutor.AnyAsync(query, cancellationToken);
    }
}