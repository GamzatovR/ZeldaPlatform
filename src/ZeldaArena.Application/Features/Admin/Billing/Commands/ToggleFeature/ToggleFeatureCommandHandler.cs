using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.ToggleFeature;

/// <summary>
/// Переключает фичу и сбрасывает кэш прав: доступ обязан измениться сразу,
/// а не через пять минут TTL (docs/SPEC.md §7.3).
/// </summary>
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