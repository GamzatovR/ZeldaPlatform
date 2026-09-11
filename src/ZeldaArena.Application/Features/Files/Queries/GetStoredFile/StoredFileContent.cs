namespace ZeldaArena.Application.Features.Files.Queries.GetStoredFile;

/// <summary>
/// Открытый файл. Поток закрывает тот, кто его отдаёт: контроллер передаёт его
/// в <c>FileStreamResult</c>, и тот освобождает поток после отправки ответа.
/// </summary>
public sealed record StoredFileContent(Stream Content, string ContentType);