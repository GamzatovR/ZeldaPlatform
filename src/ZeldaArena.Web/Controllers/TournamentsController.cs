using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;
using ZeldaArena.Web.Models.Tournaments;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Турниры: список с фильтром и страница турнира с вкладками матчей
/// (docs/SPEC.md §9.3, п. 3–4). Контроллер отправляет запросы и выбирает
/// представление — логики здесь нет (§5.2, правило 4).
///
/// Состояние списка целиком в адресе: фильтр — это GET-форма, запрос биндится
/// прямо из query-string, и F5 или пересланная ссылка восстанавливают его
/// полностью, в том числе без JavaScript (§9.1, §10.2).
/// </summary>
[Route("tournaments")]
public sealed class TournamentsController(ISender sender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTournamentsQuery filter, CancellationToken cancellationToken)
    {
        // Значение, которое не биндится (неизвестный статус, дата не в формате), —
        // это испорченная ссылка, а не пустой фильтр: молча выбросить его значило бы
        // показать не то, о чём просили.
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