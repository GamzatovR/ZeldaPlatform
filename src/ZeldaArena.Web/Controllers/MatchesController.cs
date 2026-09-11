using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Страница матча (docs/SPEC.md §9.3, п. 5). Счёт на ней пока статичен: живым его
/// сделает SignalR в Фазе 10, разметка под это уже готова (<c>data-match-id</c>,
/// <c>aria-live</c>).
/// </summary>
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