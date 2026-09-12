using System.Text.RegularExpressions;

namespace ZeldaArena.Application.Common.Files;

public static class ImageUploadRules
{
    /// <summary>2 МБ: логотипу и аватару больше не нужно, а сервер не должен принимать что попало.</summary>
    public const long MaxSizeBytes = 2 * 1024 * 1024;

    public const long MaxRequestBytes = 10 * 1024 * 1024;

    public const string StoredNamePattern = @"^[0-9a-f]{32}\.(png|jpg|webp)$";

    /// <summary>Строка для атрибута <c>accept</c> поля выбора файла.</summary>
    public const string AcceptAttribute = ".png,.jpg,.jpeg,.webp";

    public static readonly IReadOnlySet<string> AllowedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp" };

    public static bool IsStoredName(string? name) =>
        !string.IsNullOrEmpty(name) && Regex.IsMatch(name, StoredNamePattern, RegexOptions.CultureInvariant);

    public static bool HasAllowedExtension(string? fileName) =>
        !string.IsNullOrWhiteSpace(fileName) && AllowedExtensions.Contains(Path.GetExtension(fileName));

    public static string ExtensionOf(ImageFormat format) => format switch
    {
        ImageFormat.Png => ".png",
        ImageFormat.Jpeg => ".jpg",
        ImageFormat.Webp => ".webp",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };

    public static string ContentTypeOf(ImageFormat format) => format switch
    {
        ImageFormat.Png => "image/png",
        ImageFormat.Jpeg => "image/jpeg",
        ImageFormat.Webp => "image/webp",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };

    /// <summary>Формат по расширению сохранённого файла — для раздачи с верным Content-Type.</summary>
    public static ImageFormat? FormatOfStoredName(string storedName) =>
        Path.GetExtension(storedName).ToLowerInvariant() switch
        {
            ".png" => ImageFormat.Png,
            ".jpg" => ImageFormat.Jpeg,
            ".webp" => ImageFormat.Webp,
            _ => null,
        };
}