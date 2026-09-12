using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;

public sealed record ExpireDueSubscriptionsCommand : ICommand<int>, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Subscription;

    /// <summary>Сценарий работает над многими записями сразу, одной сущности у него нет.</summary>
    public string? AuditEntityId => null;
}