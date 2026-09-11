using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Payments;

namespace ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;

/// <summary>
/// Оформление заказа (docs/SPEC.md §9.3, п. 14): адрес доставки и реквизиты карты
/// одной формой, код подтверждения — на почту (§7.6).
///
/// Ни товаров, ни цен, ни суммы в команде нет: состав берётся из корзины покупателя,
/// цены — у товаров на сервере (§15, §20 п. 7). Покупатель — из текущего запроса.
/// Номер карты и CVV в аудит не попадут: имена закрыты <c>SensitiveProperties</c>.
/// </summary>
public sealed record PlaceOrderCommand(
    string Recipient,
    string Phone,
    string Country,
    string City,
    string Street,
    string PostalCode,
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv,
    string ConfirmationEmail,
    string IdempotencyKey)
    : ICommand<StartPaymentResult>, IAuditableRequest, ICardPaymentDetails
{
    public string AuditEntityType => ShopAudit.Order;

    /// <summary>Идентификатора заказа на момент вызова ещё нет — он только создаётся.</summary>
    public string? AuditEntityId => null;
}