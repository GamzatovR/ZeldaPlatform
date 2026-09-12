namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal static class SampleImages
{
    public static byte[] Png { get; } = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, 0x49, 0x48, 0x44, 0x52];

    public static byte[] Jpeg { get; } = [0xFF, 0xD8, 0xFF, 0xE0, 0, 16, 0x4A, 0x46, 0x49, 0x46, 0, 1];

    public static byte[] Webp { get; } = [.. "RIFF"u8, 0x24, 0, 0, 0, .. "WEBP"u8, .. "VP8 "u8];

    /// <summary>Заголовок исполняемого файла Windows — классика «переименовал в .png».</summary>
    public static byte[] Executable { get; } = [0x4D, 0x5A, 0x90, 0, 3, 0, 0, 0, 4, 0, 0, 0, 0xFF, 0xFF];

    public static Stream AsStream(this byte[] bytes) => new MemoryStream(bytes);
}