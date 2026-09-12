using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Orders.Commands.ChangeOrderStatus;

public sealed record ChangeOrderStatusCommand(string Number, OrderTransition Transition)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Order;

    public string? AuditEntityId => Number;
}