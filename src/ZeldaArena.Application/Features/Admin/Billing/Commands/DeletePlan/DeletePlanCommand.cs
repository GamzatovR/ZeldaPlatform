using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.DeletePlan;

public sealed record DeletePlanCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Plan;

    public string? AuditEntityId => Id.ToString();
}