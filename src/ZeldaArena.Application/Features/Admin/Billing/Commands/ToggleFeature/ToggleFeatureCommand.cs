using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.ToggleFeature;

/// <summary>
/// Включает или выключает платную функцию. Выключенная фича перестаёт давать права
/// всем сразу, оставаясь привязанной к тарифам, — это самый быстрый способ закрыть
/// функцию, не разбирая тарифы (docs/SPEC.md §7.3).
/// </summary>
public sealed record ToggleFeatureCommand(Guid FeatureId, bool IsActive)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Feature;

    public string? AuditEntityId => FeatureId.ToString();
}