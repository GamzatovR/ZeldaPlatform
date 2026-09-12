using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Orders;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.UpdateProduct;

/// <summary>
/// Правка товара: название, категория, описание, цена, остаток, наличие в продаже.
/// Артикул не меняется — по нему товар ищут в учёте.
/// </summary>
public sealed record UpdateProductCommand : ICommand, IAuditableRequest, IProductFields
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public Guid CategoryId { get; init; }

    public decimal Price { get; init; }

    public int StockQuantity { get; init; }

    public string? Description { get; init; }

    public bool IsActive { get; init; }

    public FileUpload? Image { get; init; }

    public bool RemoveImage { get; init; }

    public string AuditEntityType => ShopAudit.Product;

    public string? AuditEntityId => Id.ToString();
}