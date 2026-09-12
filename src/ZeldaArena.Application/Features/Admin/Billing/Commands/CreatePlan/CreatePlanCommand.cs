using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.CreatePlan;

public sealed record CreatePlanCommand(
    string Code,
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    int SortOrder)
    : ICommand<Guid>, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Plan;

    public string? AuditEntityId => null;
}