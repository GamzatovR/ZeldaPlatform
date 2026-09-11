using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Common.Files;

namespace ZeldaArena.Web.Extensions;

/// <summary>
/// Адрес картинки товара. У товаров сида это статический путь от корня сайта
/// (фотографии из макета), у товаров из админки (Фаза 9) — имя файла в хранилище,
/// которое раздаёт <c>FilesController</c>. Всё прочее картинкой не считается:
/// произвольная строка из базы не должна превращаться в чужой URL.
/// </summary>
public static class ProductImageUrlExtensions
{
    private const string StaticImagePrefix = "/img/";

    public static string? ProductImage(this IUrlHelper url, string? imagePath)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (string.IsNullOrEmpty(imagePath))
        {
            return null;
        }

        if (imagePath.StartsWith(StaticImagePrefix, StringComparison.Ordinal) && !imagePath.Contains("..", StringComparison.Ordinal))
        {
            return url.Content("~" + imagePath);
        }

        return ImageUploadRules.IsStoredName(imagePath) ? url.StoredFile(imagePath) : null;
    }
}