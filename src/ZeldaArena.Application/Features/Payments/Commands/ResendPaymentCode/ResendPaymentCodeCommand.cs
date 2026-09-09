using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Payments.Commands.ResendPaymentCode;

/// <summary>
/// Повторная отправка кода (docs/SPEC.md §7.6, шаг 3): письмо не дошло, попало
/// в спам или код истёк.
///
/// Не чаще раза в минуту. Ограничение проверяется здесь, а не только политикой
/// rate limiting: та считает запросы с адреса, а эта пауза привязана к платежу,
/// поэтому её не обойти вторым окном браузера.
/// </summary>
public sealed record ResendPaymentCodeCommand(Guid PaymentId) : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Payment;

    public string? AuditEntityId => PaymentId.ToString();
}