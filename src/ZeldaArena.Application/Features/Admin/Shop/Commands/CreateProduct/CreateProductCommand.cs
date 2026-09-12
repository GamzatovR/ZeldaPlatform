using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.CreateProduct;

/// <summary>Новый товар (docs/SPEC.md §9.4, п. 6).</summary>
public sealed record CreateProductCommand : ICommand<Guid>, IAuditableRequest, IProductFields
{
    public required string Sku { get; init; }

    public required string Name { get; init; }

    public Guid CategoryId { get; init; }

    public decimal Price { get; init; }

    public int StockQuantity { get; init; }

    public string? Description { get; init; }

    public FileUpload? Image { get; init; }

    public string AuditEntityType => ShopAudit.Product;

    public string? AuditEntityId => null;
}