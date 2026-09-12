using MediatR;

using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;

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