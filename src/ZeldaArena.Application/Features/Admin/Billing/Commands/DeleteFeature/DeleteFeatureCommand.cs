using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.DeleteFeature;

public sealed record DeleteFeatureCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Feature;

    public string? AuditEntityId => Id.ToString();
}