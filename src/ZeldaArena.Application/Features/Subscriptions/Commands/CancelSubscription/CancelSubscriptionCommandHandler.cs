using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Subscriptions.Commands.CancelSubscription;

/// <summary>
/// Выключает автопродление у действующей подписки (docs/SPEC.md §7.5, п. 5).
///
/// Права при этом не трогаются и кэш не сбрасывается намеренно: набор фич
/// не изменился, подписка продолжает действовать до <c>EndsAt</c>. Доступ пропадёт
/// сам, когда срок кончится, — это увидит и <c>EntitlementResolver</c> по дате,
/// и фоновая служба истечения.
/// </summary>
public sealed class CancelSubscriptionCommandHandler(
    ICurrentUserService currentUser,
    IRepository<Subscription> subscriptions,
    IReadRepository<Subscription> subscriptionsForRead,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
    : IRequestHandler<CancelSubscriptionCommand, Result>
{
    public async Task<Result> Handle(
        CancelSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var now = clock.UtcNow;

        var query = subscriptionsForRead.Query()
            .Where(subscription => subscription.UserId == userId
                && subscription.Status == SubscriptionStatus.Active
                && subscription.EndsAt > now)
            .OrderByDescending(subscription => subscription.EndsAt)
            .Select(subscription => (Guid?)subscription.Id);

        var activeId = await queryExecutor.FirstOrDefaultAsync(query, cancellationToken)
            .ConfigureAwait(false);

        if (activeId is null)
        {
            return Result.Failure(BillingErrors.SubscriptionNotActive);
        }

        var subscription = await subscriptions
            .GetByIdAsync(activeId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (subscription is null)
        {
            return Result.Failure(BillingErrors.SubscriptionNotFound);
        }

        subscription.Cancel(now);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}