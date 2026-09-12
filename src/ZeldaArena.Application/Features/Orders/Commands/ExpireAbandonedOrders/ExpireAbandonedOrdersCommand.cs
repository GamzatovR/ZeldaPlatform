using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Orders.Commands.ExpireAbandonedOrders;

public sealed record ExpireAbandonedOrdersCommand : ICommand<int>, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Order;

    /// <summary>Сценарий работает над многими заказами сразу, одной сущности у него нет.</summary>
    public string? AuditEntityId => null;
}