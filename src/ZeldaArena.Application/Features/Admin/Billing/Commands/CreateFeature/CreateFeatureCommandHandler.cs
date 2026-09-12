using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.CreateFeature;

public sealed class CreateFeatureCommandHandler(
    IRepository<Feature> features,
    IReadRepository<Feature> featuresForRead,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateFeatureCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateFeatureCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var code = request.Code.Trim().ToLowerInvariant();

        var taken = await queryExecutor
            .AnyAsync(featuresForRead.Query().Where(feature => feature.Code == code), cancellationToken)
            .ConfigureAwait(false);

        if (taken)
        {
            return Result.Failure<Guid>(BillingErrors.FeatureCodeTaken);
        }

        var feature = Feature.Create(code, request.Name, request.Description);

        await features.AddAsync(feature, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(feature.Id);
    }
}