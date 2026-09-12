using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;

public sealed record ConfirmPaymentCommand(Guid PaymentId, string ConfirmationCode)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Payment;

    public string? AuditEntityId => PaymentId.ToString();
}