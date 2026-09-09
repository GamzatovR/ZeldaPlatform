using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Subscriptions.Commands.CancelSubscription;

/// <summary>
/// Отказ от автопродления (docs/SPEC.md §7.5, п. 5). Доступ сохраняется до конца
/// оплаченного срока — деньги уже уплачены, отбирать функции сразу нечестно.
///
/// Подписка не передаётся: отменяется своя, найденная по текущему пользователю.
/// Иначе чужую подписку отменял бы любой, кто подставит идентификатор.
/// </summary>
public sealed record CancelSubscriptionCommand : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Subscription;

    public string? AuditEntityId => null;
}