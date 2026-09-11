namespace ZeldaArena.Application.Common.Files;

/// <summary>
/// Определяет формат изображения по magic bytes (docs/SPEC.md §15).
///
/// Живёт в Application, а не рядом с хранилищем в Infrastructure: это правило,
/// а не транспорт. Его читают и сценарий загрузки, отвечающий пользователю понятной
/// ошибкой, и само хранилище, которое перепроверяет вход. Здесь же его покрывают
/// unit-тесты — <c>ZeldaArena.UnitTests</c> ссылается только на Domain и Application,
/// как и в случае <c>EntitlementResolver</c> из Фазы 4.
///
/// Расширение имени и заявленный Content-Type подконтрольны клиенту и не значат
/// ничего: исполняемый файл, переименованный в <c>logo.png</c>, отсюда не пройдёт.
/// </summary>
public static class ImageFileInspector
{
    /// <summary>Сколько байт начала файла нужно, чтобы распознать любой из форматов.</summary>
    public const int HeaderLength = 12;

    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static ReadOnlySpan<byte> JpegSignature => [0xFF, 0xD8, 0xFF];

    // WebP — контейнер RIFF: «RIFF», четыре байта длины, затем «WEBP».
    private static ReadOnlySpan<byte> RiffSignature => "RIFF"u8;

    private static ReadOnlySpan<byte> WebpSignature => "WEBP"u8;

    public static ImageFormat? Detect(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith(PngSignature))
        {
            return ImageFormat.Png;
        }

        if (header.StartsWith(JpegSignature))
        {
            return ImageFormat.Jpeg;
        }

        if (header.Length >= HeaderLength
            && header.StartsWith(RiffSignature)
            && header[8..12].SequenceEqual(WebpSignature))
        {
            return ImageFormat.Webp;
        }

        return null;
    }

    /// <summary>
    /// Читает начало потока и возвращает его позицию на место. Поток обязан поддерживать
    /// перемотку: загруженный файл ASP.NET Core буферизует, и такой поток её умеет.
    /// </summary>
    public static async Task<ImageFormat?> DetectAsync(Stream content, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanSeek)
        {
            throw new ArgumentException("Для проверки сигнатуры поток должен поддерживать перемотку.", nameof(content));
        }

        var start = content.Position;
        var header = new byte[HeaderLength];
        var read = await content.ReadAtLeastAsync(header, HeaderLength, throwOnEndOfStream: false, cancellationToken)
            .ConfigureAwait(false);

        content.Position = start;

        return Detect(header.AsSpan(0, read));
    }
}