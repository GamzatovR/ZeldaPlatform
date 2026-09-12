using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Files;

public static class FileErrors
{
    public static readonly Error InvalidImage =
        new("file.invalid_image", "Файл не является изображением PNG, JPEG или WebP.");
}