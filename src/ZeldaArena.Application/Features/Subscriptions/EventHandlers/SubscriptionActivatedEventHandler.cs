using MediatR;

using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Events;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.Application.Features.Subscriptions.EventHandlers;

public sealed class SubscriptionActivatedEventHandler(
    IUserAccountService userAccounts,
    IEntitlementCacheInvalidator entitlementCache,
    ILogger<SubscriptionActivatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<SubscriptionActivatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<SubscriptionActivatedEvent> notification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var userId = notification.DomainEvent.UserId;

        await entitlementCache.InvalidateUserAsync(userId, cancellationToken).ConfigureAwait(false);

        var granted = await userAccounts
            .AddToRoleAsync(userId, RoleNames.Premium, cancellationToken)
            .ConfigureAwait(false);

        // Невыданный бейдж не повод откатывать оплату: подписка уже оплачена,
        // а права она даёт через фичи, а не через роль. Пишем в журнал и живём дальше.
        if (granted.IsFailure)
        {
            logger.LogWarning(
                "Не удалось выдать роль {Role} пользователю {UserId} после активации подписки: {Error}",
                RoleNames.Premium,
                userId,
                granted.Error.Code);
        }
    }
}