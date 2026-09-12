using ZeldaArena.Application.Common.Files;

namespace ZeldaArena.Application.Features.Admin.Shop;

/// <summary>Поля товара — общие для создания и правки.</summary>
public interface IProductFields
{
    string Name { get; }

    Guid CategoryId { get; }

    decimal Price { get; }

    int StockQuantity { get; }

    string? Description { get; }

    FileUpload? Image { get; }
}