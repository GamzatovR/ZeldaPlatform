using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetFeatures;

/// <summary>Все платные функции — страница /admin/features (docs/SPEC.md §9.4).</summary>
public sealed record GetFeaturesQuery : IQuery<IReadOnlyList<FeatureDto>>;