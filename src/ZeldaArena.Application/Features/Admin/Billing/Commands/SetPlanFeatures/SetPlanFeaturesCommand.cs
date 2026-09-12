using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.SetPlanFeatures;

public sealed record SetPlanFeaturesCommand(Guid PlanId, IReadOnlyList<PlanFeatureAssignment> Features)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Plan;

    public string? AuditEntityId => PlanId.ToString();
}