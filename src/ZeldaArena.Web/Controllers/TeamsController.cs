using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;
using ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;
using ZeldaArena.Web.Models.Teams;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Команды: список с фильтром и страница команды (docs/SPEC.md §9.3, п. 6–7).
/// Создание своей команды по подписке — здесь же, <c>/teams/create</c> (п. 8).
/// </summary>
[Route("teams")]
public sealed class TeamsController(ISender sender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTeamsQuery filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new TeamListViewModel { Filter = filter, Result = result });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var team = await sender.Send(new GetTeamBySlugQuery(slug), cancellationToken);

        if (team is null)
        {
            return NotFound();
        }

        // Отказ сценария — не ошибка страницы: у зрителя нет функции, и вместо блока
        // он увидит призыв оформить подписку.
        var advanced = await sender.Send(new GetTeamAdvancedStatsQuery(team.Id), cancellationToken);

        return View(new TeamDetailsViewModel
        {
            Team = team,
            AdvancedStats = advanced.IsSuccess ? advanced.Value : null,
        });
    }
}