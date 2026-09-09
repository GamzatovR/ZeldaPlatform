using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

/// <summary>
/// Первый шаг мнимой оплаты подписки (docs/SPEC.md §7.6, шаги 1–2): реквизиты карты
/// проверяются, платёж заводится в ожидании, код уходит письмом.
///
/// Плательщик в команде не передаётся — он берётся из текущего запроса, как и везде
/// в сценариях аккаунта: иначе можно было бы оплатить подписку чужому пользователю
/// или, что хуже, записать чужую карту на себя.
///
/// Номер карты и CVV живут ровно до вызова платёжного провайдера. В аудит они
/// не попадут: имена полей закрыты <c>SensitiveProperties</c>.
/// </summary>
public sealed record StartSubscriptionPaymentCommand(
    Guid PlanId,
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv,
    string ConfirmationEmail,
    string IdempotencyKey)
    : ICommand<StartPaymentResult>, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Payment;

    /// <summary>Идентификатора платежа на момент вызова ещё нет — он только создаётся.</summary>
    public string? AuditEntityId => null;
}