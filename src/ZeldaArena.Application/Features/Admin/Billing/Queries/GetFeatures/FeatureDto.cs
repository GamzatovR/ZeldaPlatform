namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetFeatures;

/// <summary>
/// Платная функция для админки. <see cref="PlanCount"/> показывает, в скольких
/// тарифах она состоит: выключать функцию, входящую в проданный тариф, стоит
/// осознанно.
/// </summary>
public sealed record FeatureDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    int PlanCount);