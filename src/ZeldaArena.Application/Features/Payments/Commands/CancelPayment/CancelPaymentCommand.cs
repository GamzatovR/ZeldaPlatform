using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Payments.Commands.CancelPayment;

public sealed record CancelPaymentCommand(Guid PaymentId) : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Payment;

    public string? AuditEntityId => PaymentId.ToString();
}