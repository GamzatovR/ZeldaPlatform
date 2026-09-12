using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(string Number) : ICommand, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Order;

    public string? AuditEntityId => Number;
}