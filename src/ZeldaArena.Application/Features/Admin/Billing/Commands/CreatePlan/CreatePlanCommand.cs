using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.CreatePlan;

/// <summary>
/// Новый тариф из админки (docs/SPEC.md §9.4, п. 8). Это половина демонстрации EP-4:
/// фичу снимают с одного тарифа и заводят под неё отдельный — без строки кода и без
/// перезапуска (§19).
/// </summary>
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