using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Matches.Queries.GetSchedule;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentOptions;
using ZeldaArena.Web.Models.Schedule;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Расписание матчей (docs/SPEC.md §9.3, п. 2). Фильтр — GET-форма, состояние в адресе (§10.2).
/// </summary>
[Route("schedule")]
public sealed class ScheduleController(ISender sender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetScheduleQuery filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var schedule = await sender.Send(filter, cancellationToken);
        var tournaments = await sender.Send(new GetTournamentOptionsQuery(), cancellationToken);

        return View(new ScheduleViewModel { Filter = filter, Schedule = schedule, Tournaments = tournaments });
    }
}