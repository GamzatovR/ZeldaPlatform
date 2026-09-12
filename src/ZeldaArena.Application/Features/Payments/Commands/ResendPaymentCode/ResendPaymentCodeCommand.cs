using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Payments.Commands.ResendPaymentCode;

public sealed record ResendPaymentCodeCommand(Guid PaymentId) : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Payment;

    public string? AuditEntityId => PaymentId.ToString();
}