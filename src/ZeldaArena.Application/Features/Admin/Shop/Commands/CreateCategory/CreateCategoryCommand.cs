using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.CreateCategory;

/// <summary>Категория товаров — плоский справочник.</summary>
public sealed record CreateCategoryCommand(string Name) : ICommand<Guid>, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Category;

    public string? AuditEntityId => null;
}