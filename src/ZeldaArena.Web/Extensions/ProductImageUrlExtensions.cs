using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Common.Files;

namespace ZeldaArena.Web.Extensions;

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