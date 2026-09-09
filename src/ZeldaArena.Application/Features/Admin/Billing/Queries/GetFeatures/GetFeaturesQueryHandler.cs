using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetFeatures;

/// <summary>
/// Отдаёт все функции, включая выключенные: админка обязана показывать и то,
/// что сейчас никому ничего не открывает.
/// </summary>
public sealed class GetFeaturesQueryHandler(
    IReadRepository<Feature> features,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetFeaturesQuery, IReadOnlyList<FeatureDto>>
{
    public Task<IReadOnlyList<FeatureDto>> Handle(
        GetFeaturesQuery request,
        CancellationToken cancellationToken)
    {
        // Count() методом, а не свойством: свойство EF в SQL не переводит и считает
        // связи в памяти — тот самый N+1, что был пойман в Фазе 2.
        var query = features.Query()
            .OrderBy(feature => feature.Code)
            .Select(feature => new FeatureDto(
                feature.Id,
                feature.Code,
                feature.Name,
                feature.Description,
                feature.IsActive,
                feature.PlanFeatures.Count()));

        return queryExecutor.ToListAsync(query, cancellationToken);
    }
}