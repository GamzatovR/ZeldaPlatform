using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Orders.Commands.ChangeOrderStatus;

/// <summary>
/// Статус заказа из админки (docs/SPEC.md §9.4, п. 7). Отмена оплаченного заказа
/// возвращает остаток на склад и возвращает деньги покупателю (docs/adr/ADR-0010).
/// </summary>
public sealed record ChangeOrderStatusCommand(string Number, OrderTransition Transition)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Order;

    public string? AuditEntityId => Number;
}