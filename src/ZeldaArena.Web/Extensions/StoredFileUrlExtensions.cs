using Microsoft.AspNetCore.Mvc;

namespace ZeldaArena.Web.Extensions;

/// <summary>
/// Адрес загруженного файла. В сущностях хранится имя файла в хранилище, а не URL:
/// путь раздачи — дело слоя представления, и домен о нём знать не должен.
/// </summary>
public static class StoredFileUrlExtensions
{
    public static string? StoredFile(this IUrlHelper url, string? storedName)
    {
        ArgumentNullException.ThrowIfNull(url);

        return string.IsNullOrEmpty(storedName)
            ? null
            : url.Action("Get", "Files", new { name = storedName });
    }
}