namespace ZeldaArena.Application.Common.Files;

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