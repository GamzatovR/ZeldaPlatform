using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Product;

    public string? AuditEntityId => Id.ToString();
}