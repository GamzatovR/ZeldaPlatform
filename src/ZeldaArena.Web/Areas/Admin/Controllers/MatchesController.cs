using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Matches.Commands.ChangeMatchStatus;
using ZeldaArena.Application.Features.Admin.Matches.Commands.DeleteMatch;
using ZeldaArena.Application.Features.Admin.Matches.Commands.ScheduleMatch;
using ZeldaArena.Application.Features.Admin.Matches.Commands.UpdateMatch;
using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;
using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;
using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchFormOptions;
using ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;
using ZeldaArena.Web.Areas.Admin.Models;
using ZeldaArena.Web.Areas.Admin.Models.Matches;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

[Route("admin/matches")]
[Authorize(Policy = PolicyNames.CanManageCatalog)]
public sealed class MatchesController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetMatchesForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);
        var options = await sender.Send(new GetMatchFormOptionsQuery(null), cancellationToken);

        return View(new MatchIndexViewModel
        {
            Filter = filter,
            Result = result,
            Tournaments = options.Tournaments,
        });
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(Guid? tournamentId, CancellationToken cancellationToken)
    {
        var options = await sender.Send(new GetMatchFormOptionsQuery(tournamentId), cancellationToken);

        return View(new MatchCreateViewModel
        {
            Options = options,
            Form = new MatchFormViewModel
            {
                TournamentId = tournamentId ?? Guid.Empty,
                ScheduledAt = DateTime.UtcNow.Date.AddDays(1).AddHours(18),
            },
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [Bind(Prefix = nameof(MatchCreateViewModel.Form))] MatchFormViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (ModelState.IsValid)
        {
            var result = await sender.Send(
                new ScheduleMatchCommand
                {
                    TournamentId = model.TournamentId,
                    TeamAId = model.TeamAId,
                    TeamBId = model.TeamBId,
                    ScheduledAt = UtcInput.ToUtc(model.ScheduledAt),
                    BestOf = model.BestOf,
                    StreamUrl = model.StreamUrl,
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                ReportSuccess(localizer["admin.match.created"].Value);

                return RedirectToAction(nameof(Console), new { id = result.Value });
            }

            ModelState.AddResultError(result, localizer);
        }

        var options = await sender.Send(new GetMatchFormOptionsQuery(model.TournamentId), cancellationToken);

        return View(new MatchCreateViewModel { Options = options, Form = model });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Console(Guid id, CancellationToken cancellationToken)
    {
        var match = await sender.Send(new GetMatchConsoleQuery(id), cancellationToken);

        return match is null
            ? NotFound()
            : View(new MatchConsoleViewModel
            {
                Match = match,
                Settings = new MatchSettingsViewModel
                {
                    ScheduledAt = UtcInput.FromUtc(match.ScheduledAt),
                    StreamUrl = match.StreamUrl,
                },
            });
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Console(
        Guid id,
        [Bind(Prefix = nameof(MatchConsoleViewModel.Settings))] MatchSettingsViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (ModelState.IsValid)
        {
            var result = await sender.Send(
                new UpdateMatchCommand(id, UtcInput.ToUtc(model.ScheduledAt), model.StreamUrl),
                cancellationToken);

            if (result.IsSuccess)
            {
                ReportSuccess(localizer["admin.saved"].Value);

                return RedirectToAction(nameof(Console), new { id });
            }

            ModelState.AddResultError(result, localizer);
        }

        var match = await sender.Send(new GetMatchConsoleQuery(id), cancellationToken);

        return match is null
            ? NotFound()
            : View(new MatchConsoleViewModel { Match = match, Settings = model });
    }

    [HttpPost("{id:guid}/score")]
    public async Task<IActionResult> Score(Guid id, ScoreInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!ModelState.IsValid)
        {
            ReportFailure(FirstModelError());
        }
        else
        {
            var result = await sender.Send(
                new UpdateMatchScoreCommand(id, input.ScoreA, input.ScoreB, input.ExpectedScoreA, input.ExpectedScoreB),
                cancellationToken);

            Report(result, localizer["admin.match.score_saved"].Value, localizer);
        }

        return RedirectToAction(nameof(Console), new { id });
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, MatchTransition transition, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(new ChangeMatchStatusCommand(id, transition), cancellationToken);
        Report(result, localizer["admin.match.status_changed"].Value, localizer);

        return RedirectToAction(nameof(Console), new { id });
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteMatchCommand(id), cancellationToken);
        Report(result, localizer["admin.match.deleted"].Value, localizer);

        return result.IsSuccess
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Console), new { id });
    }
}