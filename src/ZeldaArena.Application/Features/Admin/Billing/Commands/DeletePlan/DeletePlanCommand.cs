using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.DeletePlan;

/// <summary>
/// Удаление тарифа, которым никто не пользовался (docs/adr/ADR-0010). Тариф с подписками
/// снимается с продажи, а не удаляется: подписка хранит ссылку на него и цену на момент
/// покупки.
/// </summary>
public sealed record DeletePlanCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Plan;

    public string? AuditEntityId => Id.ToString();
}