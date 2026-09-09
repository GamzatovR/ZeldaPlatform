using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;

/// <summary>
/// Подтверждение оплаты кодом из письма (docs/SPEC.md §7.6, шаг 4).
///
/// Владелец платежа не передаётся: он берётся из текущего запроса и сверяется
/// с плательщиком — иначе чужой платёж подтверждался бы подбором идентификатора
/// (§15, защита от IDOR).
///
/// Код в аудит не попадёт: имя поля закрыто фрагментом confirmationcode
/// в <c>SensitiveProperties</c>.
/// </summary>
public sealed record ConfirmPaymentCommand(Guid PaymentId, string ConfirmationCode)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Payment;

    public string? AuditEntityId => PaymentId.ToString();
}