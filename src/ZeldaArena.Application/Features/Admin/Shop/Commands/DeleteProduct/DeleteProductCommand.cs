using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteProduct;

/// <summary>
/// Удаление товара, который ни разу не покупали (docs/adr/ADR-0010). Проданный товар
/// снимается с продажи: на него ссылаются позиции заказов.
/// </summary>
public sealed record DeleteProductCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Product;

    public string? AuditEntityId => Id.ToString();
}