using MediatR;

using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Events;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.Application.Features.Subscriptions.EventHandlers;

/// <summary>
/// Подписка активирована или продлена (docs/SPEC.md §7.5, п. 3): выдаём роль Premium
/// и сбрасываем кэш прав.
///
/// Почему это не в хендлере оплаты. Подписка активируется не только оплатой:
/// её может выдать администратор, и продление приходит тем же событием. Обязанность
/// «после активации — бейдж и свежие права» одна, и живёт она в одном месте
/// (§5.5, SRP).
///
/// Роль Premium здесь — исключительно бейдж у ника. Доступ к платным функциям она
/// не открывает: его определяет только <c>IEntitlementService</c> (§7.4, §20 пункт 2,
/// docs/adr/ADR-0005).
///
/// Сброс кэша обязателен: TTL в пять минут терпим при истечении подписки, но
/// недопустим сразу после оплаты — пользователь смотрит на результат прямо сейчас.
/// </summary>
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