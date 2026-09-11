using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Хранилище над словарём. Повторяет контракт настоящего: имя — GUID и расширение
/// распознанного формата, содержимое не изображения отвергается исключением.
/// </summary>
internal sealed class InMemoryFileStorage : IFileStorage
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, byte[]> Files => _files;

    public List<string> Deleted { get; } = [];

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var format = await ImageFileInspector.DetectAsync(content, cancellationToken)
            ?? throw new InvalidDataException("not an image");

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);

        var name = Guid.NewGuid().ToString("N") + ImageUploadRules.ExtensionOf(format);
        _files[name] = buffer.ToArray();

        return new StoredFile(name, ImageUploadRules.ContentTypeOf(format), buffer.Length);
    }

    public Task<Stream?> OpenAsync(string storedPath, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream?>(_files.TryGetValue(storedPath, out var bytes) ? new MemoryStream(bytes) : null);

    public Task DeleteAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        _files.Remove(storedPath);
        Deleted.Add(storedPath);

        return Task.CompletedTask;
    }
}