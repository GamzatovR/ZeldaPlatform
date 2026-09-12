using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Common.Files;

/// <summary>
/// Картинка сущности (логотип, аватар, фото товара) и её файл на диске должны меняться
/// вместе. База и файловое хранилище в одну транзакцию не входят, поэтому порядок такой:
/// новый файл пишется до сохранения сущности и удаляется, если сохранение не удалось;
/// прежний файл удаляется только после успешного сохранения. Хуже осиротевшего файла
/// только сущность, ссылающаяся на удалённый.
///
/// Тот же порядок уже есть у логотипа своей команды (Фаза 6); админке он нужен для
/// турниров, команд, игроков и товаров, поэтому вынесен сюда.
/// </summary>
public static class StoredImages
{
    /// <summary>Загрузки нет или это действительно картинка — по содержимому, а не по имени.</summary>
    public static async Task<bool> IsAcceptableAsync(FileUpload? upload, CancellationToken cancellationToken) =>
        upload is null
        || await ImageFileInspector.DetectAsync(upload.Content, cancellationToken).ConfigureAwait(false) is not null;

    /// <param name="storage">Хранилище файлов.</param>
    /// <param name="upload">Новая картинка или <see langword="null"/>, если её не меняют.</param>
    /// <param name="remove">Убрать текущую картинку без замены.</param>
    /// <param name="previous">Текущий путь картинки у сущности.</param>
    /// <param name="apply">Записывает в сущность новый путь (или <see langword="null"/>).</param>
    /// <param name="persist">Сохранение сущности.</param>
    /// <param name="cancellationToken">Отмена.</param>
    public static async Task ReplaceAsync(
        IFileStorage storage,
        FileUpload? upload,
        bool remove,
        string? previous,
        Action<string?> apply,
        Func<Task> persist,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(apply);
        ArgumentNullException.ThrowIfNull(persist);

        if (upload is null && !remove)
        {
            await persist().ConfigureAwait(false);
            return;
        }

        var stored = upload is null
            ? null
            : await storage.SaveAsync(upload.Content, upload.FileName, upload.ContentType, cancellationToken)
                .ConfigureAwait(false);

        try
        {
            apply(stored?.StoredPath);
            await persist().ConfigureAwait(false);
        }
        catch
        {
            if (stored is not null)
            {
                await storage.DeleteAsync(stored.StoredPath, CancellationToken.None).ConfigureAwait(false);
            }

            throw;
        }

        if (previous is not null && previous != stored?.StoredPath)
        {
            await storage.DeleteAsync(previous, cancellationToken).ConfigureAwait(false);
        }
    }
}