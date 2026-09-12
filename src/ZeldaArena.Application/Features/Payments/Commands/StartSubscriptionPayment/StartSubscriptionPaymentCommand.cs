using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

public sealed record StartSubscriptionPaymentCommand(
    Guid PlanId,
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv,
    string ConfirmationEmail,
    string IdempotencyKey)
    : ICommand<StartPaymentResult>, IAuditableRequest, ICardPaymentDetails
{
    public string AuditEntityType => BillingAudit.Payment;

    /// <summary>Идентификатора платежа на момент вызова ещё нет — он только создаётся.</summary>
    public string? AuditEntityId => null;
}