using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;
using ZeldaArena.Web.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>
/// Фильтр и пагинация турниров, вкладки матчей турнира (docs/SPEC.md §10.1, сценарии 1–3).
/// Запросы те же, что у <see cref="TournamentsController"/>; ответ — тот же partial,
/// которым страница рисует список, поэтому разметка у обоих путей одна.
/// </summary>
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

    /// <summary>
    /// Вкладка матчей. Турнир адресуется слагом, как и его страница: ссылки пагинации
    /// внутри вкладки ведут на <c>/tournaments/{slug}?state=…&amp;page=…</c>.
    /// </summary>
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