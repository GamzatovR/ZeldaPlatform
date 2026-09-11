namespace ZeldaArena.Application.Common.Files;

/// <summary>
/// Форматы изображений, которые принимает хранилище (docs/SPEC.md §15). Каждый
/// определяется по сигнатуре содержимого, а не по расширению и не по заявленному типу.
/// </summary>
public enum ImageFormat
{
    Png = 1,
    Jpeg = 2,
    Webp = 3,
}