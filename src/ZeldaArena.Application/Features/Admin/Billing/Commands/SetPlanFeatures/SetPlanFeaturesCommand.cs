using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.SetPlanFeatures;

/// <summary>
/// Задаёт состав фич тарифа — главный жест демонстрации EP-4 (docs/SPEC.md §5.4):
/// администратор снимает <c>stats.advanced</c> с тарифа Pro и продаёт её отдельным
/// тарифом, не трогая код и не выкладывая новую версию.
///
/// Передаётся весь набор целиком, а не «добавить одну». Так чекбоксы в админке
/// отправляются одной формой, и не нужен отдельный сценарий на снятие.
/// </summary>
public sealed record SetPlanFeaturesCommand(Guid PlanId, IReadOnlyList<PlanFeatureAssignment> Features)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Plan;

    public string? AuditEntityId => PlanId.ToString();
}