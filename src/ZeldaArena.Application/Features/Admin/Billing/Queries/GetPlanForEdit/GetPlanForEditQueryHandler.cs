using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;

public sealed class GetPlanForEditQueryHandler(
    IReadRepository<Plan> plans,
    IReadRepository<Feature> features,
    IReadRepository<Subscription> subscriptions,
    IQueryExecutor queryExecutor,
    IDateTimeProvider clock)
    : IRequestHandler<GetPlanForEditQuery, PlanEditDto?>
{
    public async Task<PlanEditDto?> Handle(GetPlanForEditQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = request.Id;
        var now = clock.UtcNow;

        var plan = await queryExecutor.FirstOrDefaultAsync(
            plans.Query()
                .Where(item => item.Id == id)
                .Select(item => new
                {
                    Dto = new PlanEditDto
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price.Amount,
                        Currency = item.Price.Currency,
                        DurationDays = item.DurationDays,
                        SortOrder = item.SortOrder,
                        IsActive = item.IsActive,
                        ActiveSubscriptions = subscriptions.Query().Count(subscription =>
                            subscription.PlanId == id
                            && subscription.Status == SubscriptionStatus.Active
                            && subscription.EndsAt > now),
                    },
                    Granted = item.PlanFeatures.Select(planFeature => new { planFeature.FeatureId, planFeature.Value }).ToList(),
                }),
            cancellationToken);

        if (plan is null)
        {
            return null;
        }

        var all = await queryExecutor.ToListAsync(
            features.Query()
                .OrderBy(feature => feature.Code)
                .Select(feature => new { feature.Id, feature.Code, feature.Name, feature.IsActive }),
            cancellationToken);

        var options = all
            .Select(feature =>
            {
                var granted = plan.Granted.Find(item => item.FeatureId == feature.Id);

                return new PlanFeatureOptionDto(
                    feature.Id,
                    feature.Code,
                    feature.Name,
                    feature.IsActive,
                    granted is not null,
                    granted?.Value);
            })
            .ToList();

        return plan.Dto with { Features = options };
    }
}