using System.Text.RegularExpressions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Infrastructure.Files;

/// <summary>Хранилище на локальном диске.</summary>
public sealed partial class LocalFileStorage : IFileStorage
{
    private const int CopyBufferSize = 81920;

    private readonly string _root;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(
        IOptions<FileStorageOptions> options,
        IHostEnvironment environment,
        ILogger<LocalFileStorage> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        _root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.RootPath));
        _logger = logger;
    }

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var format = await ImageFileInspector.DetectAsync(content, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidDataException("Содержимое не является разрешённым изображением.");

        // Имя и расширение — свои: исходное имя клиента не попадает на диск ни целиком,
        // ни частью, поэтому ни «../», ни двойное расширение ничего не дают.
        var storedName = Guid.NewGuid().ToString("N") + ImageUploadRules.ExtensionOf(format);
        var fullPath = Path.Combine(_root, storedName);

        Directory.CreateDirectory(_root);

        long written;
        var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, CopyBufferSize, useAsync: true);
        await using (target.ConfigureAwait(false))
        {
            written = await CopyWithLimitAsync(content, target, cancellationToken).ConfigureAwait(false);
        }

        if (written > ImageUploadRules.MaxSizeBytes)
        {
            File.Delete(fullPath);

            throw new InvalidDataException($"Файл больше {ImageUploadRules.MaxSizeBytes} байт.");
        }

        _logger.LogInformation("Сохранён файл {StoredName}, {Size} байт", storedName, written);

        return new StoredFile(storedName, ImageUploadRules.ContentTypeOf(format), written);
    }

    public Task<Stream?> OpenAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        if (Resolve(storedPath) is not { } fullPath || !File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize, useAsync: true);

        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        if (Resolve(storedPath) is { } fullPath && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string? Resolve(string? storedPath)
    {
        if (string.IsNullOrEmpty(storedPath) || !StoredName().IsMatch(storedPath))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(Path.Combine(_root, storedPath));

        return fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? fullPath
            : null;
    }

    private static async Task<long> CopyWithLimitAsync(Stream source, Stream target, CancellationToken cancellationToken)
    {
        var buffer = new byte[CopyBufferSize];
        long total = 0;
        int read;

        while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            total += read;

            if (total > ImageUploadRules.MaxSizeBytes)
            {
                return total;
            }

            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        return total;
    }

    [GeneratedRegex(ImageUploadRules.StoredNamePattern)]
    private static partial Regex StoredName();
}