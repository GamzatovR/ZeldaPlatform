using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.CreatePlan;

public sealed class CreatePlanCommandHandler(
    IRepository<Plan> planRepository,
    IReadRepository<Plan> plans,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePlanCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var code = request.Code.Trim().ToLowerInvariant();

        if (await queryExecutor.AnyAsync(plans.Query().Where(plan => plan.Code == code), cancellationToken))
        {
            return Result.Failure<Guid>(BillingErrors.PlanCodeTaken);
        }

        Plan? plan = null;

        var created = DomainRules.Apply(() => plan = Plan.Create(
            code,
            request.Name,
            Money.FromRubles(request.Price),
            request.DurationDays,
            request.Description,
            request.SortOrder));

        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        await planRepository.AddAsync(plan!, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return plan!.Id;
    }
}