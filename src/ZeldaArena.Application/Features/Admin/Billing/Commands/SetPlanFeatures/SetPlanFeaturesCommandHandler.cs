using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.SetPlanFeatures;

public sealed class SetPlanFeaturesCommandHandler(
    IRepository<Plan> plans,
    IReadRepository<Feature> features,
    IQueryExecutor queryExecutor,
    IEntitlementCacheInvalidator entitlementCache,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetPlanFeaturesCommand, Result>
{
    public async Task<Result> Handle(SetPlanFeaturesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var plan = await plans.GetByIdAsync(request.PlanId, cancellationToken).ConfigureAwait(false);

        if (plan is null)
        {
            return Result.Failure(BillingErrors.PlanNotFound);
        }

        var requestedIds = request.Features.Select(feature => feature.FeatureId).ToArray();

        // Несуществующая фича в наборе — это ошибка формы, а не повод завести
        // привязку в никуда: такая строка молча не давала бы никаких прав.
        var knownIds = await queryExecutor
            .ToListAsync(
                features.Query()
                    .Where(feature => requestedIds.Contains(feature.Id))
                    .Select(feature => feature.Id),
                cancellationToken)
            .ConfigureAwait(false);

        if (knownIds.Count != requestedIds.Length)
        {
            return Result.Failure(BillingErrors.FeatureNotFound);
        }

        foreach (var granted in plan.PlanFeatures.Select(planFeature => planFeature.FeatureId).ToArray())
        {
            if (!requestedIds.Contains(granted))
            {
                plan.RevokeFeature(granted);
            }
        }

        // Повторная привязка обновляет параметр, а не дублирует строку —
        // об этом заботится сам тариф.
        foreach (var assignment in request.Features)
        {
            plan.GrantFeature(assignment.FeatureId, assignment.Value);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await entitlementCache.InvalidateAllAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}