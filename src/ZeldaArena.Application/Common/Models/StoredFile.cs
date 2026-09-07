namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Файл, принятый хранилищем (docs/SPEC.md §15): исходное имя не сохраняется как есть,
/// файл переименован в GUID и лежит вне <c>wwwroot</c>, раздаётся через контроллер.
/// </summary>
public sealed record StoredFile(string StoredPath, string ContentType, long SizeBytes);