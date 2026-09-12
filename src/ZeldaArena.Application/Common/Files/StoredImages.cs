using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Common.Files;

public static class StoredImages
{
    /// <summary>Загрузки нет или это действительно картинка — по содержимому, а не по имени.</summary>
    public static async Task<bool> IsAcceptableAsync(FileUpload? upload, CancellationToken cancellationToken) =>
        upload is null
        || await ImageFileInspector.DetectAsync(upload.Content, cancellationToken).ConfigureAwait(false) is not null;

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