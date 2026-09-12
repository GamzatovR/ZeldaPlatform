using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;
using ZeldaArena.Web.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers;

[Route("api/tournaments")]
public sealed class TournamentsApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTournamentsQuery filter, CancellationToken cancellationToken)
    {
        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            "~/Views/Tournaments/_TournamentList.cshtml",
            result,
            PageUrl(nameof(TournamentsController.Index), "Tournaments"));
    }

    [HttpGet("{slug}/matches")]
    public async Task<IActionResult> Matches(
        string slug,
        [FromQuery] MatchListState state,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        var tournament = await sender.Send(new GetTournamentBySlugQuery(slug), cancellationToken);

        if (tournament is null)
        {
            return NotFound();
        }

        var matches = await sender.Send(
            new GetTournamentMatchesQuery { TournamentId = tournament.Id, State = state, Page = page },
            cancellationToken);

        return ListPartial(
            "~/Views/Tournaments/_TournamentMatches.cshtml",
            matches,
            PageUrl(nameof(TournamentsController.Details), "Tournaments", new { slug }));
    }
}