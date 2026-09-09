using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Payments.Commands.CancelPayment;

/// <summary>
/// Отказ от оплаты до подтверждения: пользователь передумал или решил начать заново
/// (docs/SPEC.md §7.6, шаг 4).
/// </summary>
public sealed record CancelPaymentCommand(Guid PaymentId) : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Payment;

    public string? AuditEntityId => PaymentId.ToString();
}