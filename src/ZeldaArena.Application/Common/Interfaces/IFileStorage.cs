using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Хранилище загруженных файлов: логотипы команд, аватары, изображения товаров.
/// Реализация — Фаза 6.
///
/// Требования к реализации зафиксированы в docs/SPEC.md §15: whitelist расширений
/// и проверка magic bytes, лимит размера, переименование в GUID, хранение вне
/// <c>wwwroot</c> и раздача через контроллер. Порт поэтому принимает и исходное имя,
/// и заявленный тип — оба нужны для проверок и ни одно не используется как имя файла.
/// </summary>
public interface IFileStorage
{
    Task<StoredFile> SaveAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenAsync(string storedPath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storedPath, CancellationToken cancellationToken = default);
}