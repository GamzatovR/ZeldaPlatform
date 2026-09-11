using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Files.Queries.GetStoredFile;

/// <summary>
/// Файл из хранилища для раздачи через контроллер (docs/SPEC.md §15: файлы лежат
/// вне <c>wwwroot</c> и напрямую веб-сервером не отдаются).
/// </summary>
/// <param name="Name">Имя сохранённого файла — то, что хранилище вернуло при загрузке.</param>
public sealed record GetStoredFileQuery(string Name) : IQuery<StoredFileContent?>;