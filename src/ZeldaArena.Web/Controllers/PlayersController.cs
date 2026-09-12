using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;
using ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;
using ZeldaArena.Application.Features.Players.Queries.GetPlayerFilterOptions;
using ZeldaArena.Application.Features.Players.Queries.GetPlayers;
using ZeldaArena.Web.Models.Players;

namespace ZeldaArena.Web.Controllers;

/// <summary>Игроки: список с фильтром и страница игрока.</summary>
[Route("players")]
public sealed class PlayersController(ISender sender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetPlayersQuery filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);
        var options = await sender.Send(new GetPlayerFilterOptionsQuery(), cancellationToken);

        return View(new PlayerListViewModel { Filter = filter, Options = options, Result = result });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var player = await sender.Send(new GetPlayerBySlugQuery(slug), cancellationToken);

        if (player is null)
        {
            return NotFound();
        }

        var advanced = await sender.Send(new GetPlayerAdvancedStatsQuery(player.Id), cancellationToken);

        return View(new PlayerDetailsViewModel
        {
            Player = player,
            AdvancedStats = advanced.IsSuccess ? advanced.Value : null,
        });
    }
}