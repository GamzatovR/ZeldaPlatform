using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.UpdatePlan;

/// <summary>
/// Кэш прав сбрасывается после сохранения: цена и срок на действующие права не влияют,
/// но снятие тарифа с продажи меняет то, что видит пользователь на странице тарифов,
/// а состав фич правится этим же экраном.
/// </summary>
public sealed class UpdatePlanCommandHandler(
    IRepository<Plan> plans,
    IEntitlementCacheInvalidator entitlementCache,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePlanCommand, Result>
{
    public async Task<Result> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var plan = await plans.GetByIdAsync(request.Id, cancellationToken);

        if (plan is null)
        {
            return Result.Failure(BillingErrors.PlanNotFound);
        }

        var updated = DomainRules.Apply(() =>
        {
            plan.UpdateDetails(request.Name, request.Description, request.SortOrder);
            plan.ChangePrice(Money.FromRubles(request.Price), request.DurationDays);

            if (request.IsActive)
            {
                plan.Activate();
            }
            else
            {
                plan.Deactivate();
            }
        });

        if (updated.IsFailure)
        {
            return updated;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await entitlementCache.InvalidateAllAsync(cancellationToken);

        return Result.Success();
    }
}