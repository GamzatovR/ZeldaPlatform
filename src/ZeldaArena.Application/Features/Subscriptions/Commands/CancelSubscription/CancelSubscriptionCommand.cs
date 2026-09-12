using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Subscriptions.Commands.CancelSubscription;

public sealed record CancelSubscriptionCommand : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Subscription;

    public string? AuditEntityId => null;
}