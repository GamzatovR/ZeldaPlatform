using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.RenameCategory;

/// <summary>Переименование категории. Слаг не меняется: по нему фильтруется каталог.</summary>
public sealed record RenameCategoryCommand(Guid Id, string Name) : ICommand, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Category;

    public string? AuditEntityId => Id.ToString();
}