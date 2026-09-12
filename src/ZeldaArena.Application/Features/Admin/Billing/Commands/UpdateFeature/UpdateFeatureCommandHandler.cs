using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.UpdateFeature;

public sealed class UpdateFeatureCommandHandler(
    IRepository<Feature> features,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateFeatureCommand, Result>
{
    public async Task<Result> Handle(UpdateFeatureCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var feature = await features.GetByIdAsync(request.Id, cancellationToken);

        if (feature is null)
        {
            return Result.Failure(BillingErrors.FeatureNotFound);
        }

        // Права не меняются: название и описание — только подпись в интерфейсе,
        // поэтому кэш прав сбрасывать не нужно.
        feature.UpdateDetails(request.Name, request.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}