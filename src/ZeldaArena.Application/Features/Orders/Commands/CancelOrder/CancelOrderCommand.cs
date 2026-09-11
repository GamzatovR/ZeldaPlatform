using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Покупатель отменяет свой неоплаченный заказ (docs/adr/ADR-0009: оплаченный отменяет
/// только администратор, Фаза 9). Отмена заказа — обязательное событие аудита §8.2.
/// </summary>
public sealed record CancelOrderCommand(string Number) : ICommand, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Order;

    public string? AuditEntityId => Number;
}