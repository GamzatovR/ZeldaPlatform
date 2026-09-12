using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;
using ZeldaArena.Web.Models.Tournaments;

namespace ZeldaArena.Web.Controllers;

[Route("tournaments")]
public sealed class TournamentsController(ISender sender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTournamentsQuery filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new TournamentListViewModel { Filter = filter, Result = result });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(
        string slug,
        MatchListState state,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var tournament = await sender.Send(new GetTournamentBySlugQuery(slug), cancellationToken);

        if (tournament is null)
        {
            return NotFound();
        }

        var matches = await sender.Send(
            new GetTournamentMatchesQuery { TournamentId = tournament.Id, State = state, Page = page },
            cancellationToken);

        return View(new TournamentDetailsViewModel { Tournament = tournament, State = state, Matches = matches });
    }
}