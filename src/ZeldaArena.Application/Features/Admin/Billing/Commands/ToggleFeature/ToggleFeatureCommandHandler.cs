using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.ToggleFeature;

public sealed class ToggleFeatureCommandHandler(
    IRepository<Feature> features,
    IEntitlementCacheInvalidator entitlementCache,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ToggleFeatureCommand, Result>
{
    public async Task<Result> Handle(ToggleFeatureCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var feature = await features.GetByIdAsync(request.FeatureId, cancellationToken)
            .ConfigureAwait(false);

        if (feature is null)
        {
            return Result.Failure(BillingErrors.FeatureNotFound);
        }

        if (request.IsActive)
        {
            feature.Activate();
        }
        else
        {
            feature.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await entitlementCache.InvalidateAllAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}