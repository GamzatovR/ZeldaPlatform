using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Files;

/// <summary>
/// Ошибки загрузки файлов. Коды — ключи ресурсов (docs/SPEC.md §9.5).
/// </summary>
public static class FileErrors
{
    /// <summary>
    /// Содержимое не похоже ни на одно разрешённое изображение, что бы ни говорили
    /// расширение и заявленный тип.
    /// </summary>
    public static readonly Error InvalidImage =
        new("file.invalid_image", "Файл не является изображением PNG, JPEG или WebP.");
}