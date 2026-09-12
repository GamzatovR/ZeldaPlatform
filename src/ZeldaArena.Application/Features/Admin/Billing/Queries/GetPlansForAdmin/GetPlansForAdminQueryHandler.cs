using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlansForAdmin;

public sealed class GetPlansForAdminQueryHandler(
    IReadRepository<Plan> plans,
    IReadRepository<Subscription> subscriptions,
    IQueryExecutor queryExecutor,
    IDateTimeProvider clock)
    : IRequestHandler<GetPlansForAdminQuery, IReadOnlyList<AdminPlanDto>>
{
    public Task<IReadOnlyList<AdminPlanDto>> Handle(
        GetPlansForAdminQuery request,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        var query = plans.Query()
            .OrderBy(plan => plan.SortOrder)
            .Select(plan => new AdminPlanDto(
                plan.Id,
                plan.Code,
                plan.Name,
                plan.Price.Amount,
                plan.Price.Currency,
                plan.DurationDays,
                plan.IsActive,
                plan.SortOrder,
                plan.PlanFeatures.Select(planFeature => planFeature.FeatureId).ToList(),
                subscriptions.Query().Count(subscription => subscription.PlanId == plan.Id
                    && subscription.Status == SubscriptionStatus.Active
                    && subscription.EndsAt > now)));

        return queryExecutor.ToListAsync(query, cancellationToken);
    }
}