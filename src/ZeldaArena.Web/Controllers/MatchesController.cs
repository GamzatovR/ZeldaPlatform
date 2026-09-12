using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

namespace ZeldaArena.Web.Controllers;

[Route("matches")]
public sealed class MatchesController(ISender sender) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var match = await sender.Send(new GetMatchDetailsQuery(id), cancellationToken);

        return match is null ? NotFound() : View(match);
    }
}