using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Files.Queries.GetStoredFile;

namespace ZeldaArena.Web.Controllers;

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