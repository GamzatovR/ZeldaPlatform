using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteCategory;

/// <summary>Удаление пустой категории: у товара категория обязательна (docs/SPEC.md §6).</summary>
public sealed record DeleteCategoryCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Category;

    public string? AuditEntityId => Id.ToString();
}