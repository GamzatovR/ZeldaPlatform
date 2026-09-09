using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;

/// <summary>
/// Помечает истёкшие подписки (docs/SPEC.md §7.5, п. 4). Вызывается фоновой службой
/// раз в час.
///
/// Отдельная команда, а не метод службы, по двум причинам: так сценарий проходит
/// через тот же конвейер, что и остальные (транзакция, аудит, журнал), и так его
/// можно вызвать руками из админки, если служба почему-то отстала.
///
/// Возвращает число обработанных подписок — служба пишет его в журнал.
/// </summary>
public sealed record ExpireDueSubscriptionsCommand : ICommand<int>, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Subscription;

    /// <summary>Сценарий работает над многими записями сразу, одной сущности у него нет.</summary>
    public string? AuditEntityId => null;
}