using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Payments;

namespace ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;

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