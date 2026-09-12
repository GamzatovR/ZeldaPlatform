using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.DeleteFeature;

public sealed class DeleteFeatureCommandHandler(
    IRepository<Feature> features,
    IReadRepository<Plan> plans,
    IQueryExecutor queryExecutor,
    IEntitlementCacheInvalidator entitlementCache,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteFeatureCommand, Result>
{
    public async Task<Result> Handle(DeleteFeatureCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var feature = await features.GetByIdAsync(request.Id, cancellationToken);

        if (feature is null)
        {
            return Result.Failure(BillingErrors.FeatureNotFound);
        }

        if (FeatureCodes.All.Contains(feature.Code, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(BillingErrors.FeatureReferencedByCode);
        }

        if (await queryExecutor.AnyAsync(
                plans.Query().SelectMany(plan => plan.PlanFeatures).Where(link => link.FeatureId == request.Id),
                cancellationToken))
        {
            return Result.Failure(BillingErrors.FeatureInUse);
        }

        features.Remove(feature);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await entitlementCache.InvalidateAllAsync(cancellationToken);

        return Result.Success();
    }
}