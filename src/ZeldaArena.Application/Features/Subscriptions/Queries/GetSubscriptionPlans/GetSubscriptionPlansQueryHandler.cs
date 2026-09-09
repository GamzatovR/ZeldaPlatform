using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;

/// <summary>
/// Отдаёт тарифы, которые продаются, вместе с их фичами.
///
/// Выключенные фичи в состав не попадают: обещать функцию, которая никому ничего
/// не открывает, нельзя (§7.3). Проекция собирается сразу в DTO и целиком уезжает
/// в SQL (§16).
/// </summary>
public sealed class GetSubscriptionPlansQueryHandler(
    IReadRepository<Plan> plans,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetSubscriptionPlansQuery, IReadOnlyList<PlanDto>>
{
    public Task<IReadOnlyList<PlanDto>> Handle(
        GetSubscriptionPlansQuery request,
        CancellationToken cancellationToken)
    {
        var query = plans.Query()
            .Where(plan => plan.IsActive)
            .OrderBy(plan => plan.SortOrder)
            .Select(plan => new PlanDto(
                plan.Id,
                plan.Code,
                plan.Name,
                plan.Description,
                plan.Price.Amount,
                plan.Price.Currency,
                plan.DurationDays,
                plan.Price.Amount == 0m,
                plan.PlanFeatures
                    .Where(planFeature => planFeature.Feature!.IsActive)
                    .Select(planFeature => new PlanFeatureDto(
                        planFeature.Feature!.Code,
                        planFeature.Feature.Name,
                        planFeature.Feature.Description))
                    .ToList()));

        return queryExecutor.ToListAsync(query, cancellationToken);
    }
}