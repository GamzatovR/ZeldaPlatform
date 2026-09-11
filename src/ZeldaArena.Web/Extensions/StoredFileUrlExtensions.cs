using Microsoft.AspNetCore.Mvc;

namespace ZeldaArena.Web.Extensions;

/// <summary>
/// Адрес загруженного файла. В сущностях хранится имя файла в хранилище, а не URL:
/// путь раздачи — дело слоя представления, и домен о нём знать не должен.
///
/// Область указана явно: логотипы рисуются и в partial, отданных из Areas/Api, а без
/// этого адрес строился бы с текущей областью Api и получался бы пустым.
/// </summary>
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