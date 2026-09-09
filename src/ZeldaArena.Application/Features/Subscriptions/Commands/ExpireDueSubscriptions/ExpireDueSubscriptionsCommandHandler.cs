using MediatR;

using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;

/// <summary>
/// Переводит закончившиеся подписки в статус Expired (docs/SPEC.md §7.5, п. 4).
///
/// Доступ к платным функциям пропадает не здесь, а по дате: <c>Subscription.IsActiveAt</c>
/// сравнивает <c>EndsAt</c> с текущим моментом, поэтому опоздание службы на час прав
/// не продлевает. Эта команда нужна ради того, что от даты не выводится: событие,
/// снятие роли Premium и письмо.
///
/// Порция ограничена: за час их накапливаются единицы, а на первом запуске после
/// долгого простоя ограничение не даст открыть транзакцию на всю таблицу. Остаток
/// доберётся следующим запуском.
/// </summary>
public sealed class ExpireDueSubscriptionsCommandHandler(
    IReadRepository<Subscription> subscriptionsForRead,
    IRepository<Subscription> subscriptions,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    ILogger<ExpireDueSubscriptionsCommandHandler> logger)
    : IRequestHandler<ExpireDueSubscriptionsCommand, Result<int>>
{
    /// <summary>Сколько подписок обрабатывается за один запуск.</summary>
    public const int BatchSize = 200;

    public async Task<Result<int>> Handle(
        ExpireDueSubscriptionsCommand request,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        var query = subscriptionsForRead.Query()
            .Where(subscription => subscription.Status == SubscriptionStatus.Active
                && subscription.EndsAt <= now)
            .OrderBy(subscription => subscription.EndsAt)
            .Take(BatchSize)
            .Select(subscription => subscription.Id);

        var dueIds = await queryExecutor.ToListAsync(query, cancellationToken).ConfigureAwait(false);

        if (dueIds.Count == 0)
        {
            return Result.Success(0);
        }

        var expired = 0;

        foreach (var id in dueIds)
        {
            var subscription = await subscriptions.GetByIdAsync(id, cancellationToken)
                .ConfigureAwait(false);

            // Между выборкой и загрузкой подписку могли отозвать или продлить.
            // Expire в обоих случаях бросил бы исключение и уронил бы весь пакет,
            // поэтому те же два условия проверяются заранее.
            if (subscription is not { Status: SubscriptionStatus.Active } || subscription.EndsAt > now)
            {
                continue;
            }

            subscription.Expire(now);
            expired++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (expired > 0)
        {
            logger.LogInformation("Помечено истёкшими подписок: {Count}.", expired);
        }

        return Result.Success(expired);
    }
}