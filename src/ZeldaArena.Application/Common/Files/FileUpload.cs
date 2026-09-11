namespace ZeldaArena.Application.Common.Files;

/// <summary>
/// Загруженный файл в команде сценария. Порт не знает про <c>IFormFile</c>:
/// это тип ASP.NET Core, а Application от него не зависит (docs/SPEC.md §5.2).
///
/// <see cref="ToString"/> переопределён ради аудита: <c>AuditBehavior</c> пишет
/// свойства команды строками, и без этого в журнал попало бы имя типа потока,
/// а не то, что загружено.
/// </summary>
public sealed record FileUpload(Stream Content, string FileName, string ContentType, long Length)
{
    public override string ToString() => $"{FileName} ({ContentType}, {Length} B)";
}