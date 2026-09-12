using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.CreateFeature;

public sealed record CreateFeatureCommand(string Code, string Name, string? Description)
    : ICommand<Guid>, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Feature;

    public string? AuditEntityId => null;
}