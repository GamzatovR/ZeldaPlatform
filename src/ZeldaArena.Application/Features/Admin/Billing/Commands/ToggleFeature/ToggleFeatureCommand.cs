using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.ToggleFeature;

public sealed record ToggleFeatureCommand(Guid FeatureId, bool IsActive)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Feature;

    public string? AuditEntityId => FeatureId.ToString();
}