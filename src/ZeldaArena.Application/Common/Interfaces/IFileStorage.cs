using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

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