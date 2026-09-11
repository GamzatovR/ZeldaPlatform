using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Files.Queries.GetStoredFile;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Раздача загруженных файлов (docs/SPEC.md §15): хранилище лежит вне <c>wwwroot</c>,
/// и единственный путь к файлу — через этот контроллер и хранилище, которое проверяет имя.
///
/// Имя файла — GUID, и под одним именем содержимое не меняется никогда: новый логотип —
/// это новый файл. Поэтому кэш на год с <c>immutable</c>, как у версионированной статики (§16).
/// </summary>
[Route("files")]
public sealed class FilesController(ISender sender) : Controller
{
    private const int OneYearInSeconds = 365 * 24 * 60 * 60;

    [HttpGet("{name}")]
    [ResponseCache(Duration = OneYearInSeconds, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(string name, CancellationToken cancellationToken)
    {
        var file = await sender.Send(new GetStoredFileQuery(name), cancellationToken);

        if (file is null)
        {
            return NotFound();
        }

        // Тип выведен из сигнатуры при загрузке; браузеру запрещено угадывать его заново.
        Response.Headers.XContentTypeOptions = "nosniff";

        return File(file.Content, file.ContentType);
    }
}