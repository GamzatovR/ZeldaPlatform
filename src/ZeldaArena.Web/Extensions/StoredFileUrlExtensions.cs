using Microsoft.AspNetCore.Mvc;

namespace ZeldaArena.Web.Extensions;

public static class StoredFileUrlExtensions
{
    public static string? StoredFile(this IUrlHelper url, string? storedName)
    {
        ArgumentNullException.ThrowIfNull(url);

        return string.IsNullOrEmpty(storedName)
            ? null
            : url.Action("Get", "Files", new { area = string.Empty, name = storedName });
    }
}