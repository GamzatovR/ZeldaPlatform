using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.DeletePlan;

public sealed class DeletePlanCommandHandler(
    IRepository<Plan> plans,
    IReadRepository<Subscription> subscriptions,
    IQueryExecutor queryExecutor,
    IEntitlementCacheInvalidator entitlementCache,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePlanCommand, Result>
{
    public async Task<Result> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var plan = await plans.GetByIdAsync(request.Id, cancellationToken);

        if (plan is null)
        {
            return Result.Failure(BillingErrors.PlanNotFound);
        }

        // Любая подписка, даже истёкшая: она хранит историю покупки.
        if (await queryExecutor.AnyAsync(
                subscriptions.Query().Where(subscription => subscription.PlanId == request.Id),
                cancellationToken))
        {
            return Result.Failure(BillingErrors.PlanHasSubscriptions);
        }

        plans.Remove(plan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await entitlementCache.InvalidateAllAsync(cancellationToken);

        return Result.Success();
    }
}