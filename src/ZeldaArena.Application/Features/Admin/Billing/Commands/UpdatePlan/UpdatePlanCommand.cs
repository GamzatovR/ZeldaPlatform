using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.UpdatePlan;

/// <summary>
/// Правка тарифа: название, описание, порядок, цена и срок. Код не меняется — по нему
/// тариф ищут в коде и конфигурации (<c>PlanCodes</c>).
/// </summary>
public sealed record UpdatePlanCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    int SortOrder,
    bool IsActive)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Plan;

    public string? AuditEntityId => Id.ToString();
}