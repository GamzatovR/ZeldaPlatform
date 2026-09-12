using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.AddTournamentTeam;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.ChangeTournamentStatus;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.CreateTournament;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.DeleteTournament;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.RemoveTournamentTeam;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournament;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournamentTeam;
using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;
using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;
using ZeldaArena.Web.Areas.Admin.Models;
using ZeldaArena.Web.Areas.Admin.Models.Tournaments;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

[Route("admin/tournaments")]
[Authorize(Policy = PolicyNames.CanManageCatalog)]
public sealed class TournamentsController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTournamentsForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        // Неразборчивое значение фильтра (?status=bogus) — испорченная ссылка, а не пустой фильтр.
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new TournamentIndexViewModel { Filter = filter, Result = result });
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        var start = DateTime.UtcNow.Date.AddDays(7).AddHours(12);

        return View(new TournamentFormViewModel { StartsAt = start, EndsAt = start.AddDays(3) });
    }

    [HttpPost("create")]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> Create(TournamentFormViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var logo = model.Logo?.OpenReadStream();

        var result = await sender.Send(
            new CreateTournamentCommand
            {
                Name = model.Name,
                Tier = model.Tier,
                Region = model.Region,
                PrizePool = model.PrizePool,
                StartsAt = UtcInput.ToUtc(model.StartsAt),
                EndsAt = UtcInput.ToUtc(model.EndsAt),
                Description = model.Description,
                RulesHtml = model.RulesHtml,
                IsFeatured = model.IsFeatured,
                Logo = model.Logo.ToFileUpload(logo),
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return View(model);
        }

        ReportSuccess(localizer["admin.tournament.created"].Value);

        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var tournament = await sender.Send(new GetTournamentForEditQuery(id), cancellationToken);

        return tournament is null
            ? NotFound()
            : View(new TournamentEditViewModel { Tournament = tournament, Form = TournamentFormViewModel.From(tournament) });
    }

    [HttpPost("{id:guid}")]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> Edit(
        Guid id,
        [Bind(Prefix = nameof(TournamentEditViewModel.Form))] TournamentFormViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (ModelState.IsValid)
        {
            await using var logo = model.Logo?.OpenReadStream();

            var result = await sender.Send(
                new UpdateTournamentCommand
                {
                    Id = id,
                    Name = model.Name,
                    Tier = model.Tier,
                    Region = model.Region,
                    PrizePool = model.PrizePool,
                    StartsAt = UtcInput.ToUtc(model.StartsAt),
                    EndsAt = UtcInput.ToUtc(model.EndsAt),
                    Description = model.Description,
                    RulesHtml = model.RulesHtml,
                    IsFeatured = model.IsFeatured,
                    Logo = model.Logo.ToFileUpload(logo),
                    RemoveLogo = model.RemoveLogo,
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                ReportSuccess(localizer["admin.saved"].Value);

                return RedirectToAction(nameof(Edit), new { id });
            }

            ModelState.AddResultError(result, localizer);
        }

        var tournament = await sender.Send(new GetTournamentForEditQuery(id), cancellationToken);

        if (tournament is null)
        {
            return NotFound();
        }

        model.CurrentLogoPath = tournament.LogoPath;

        return View(new TournamentEditViewModel { Tournament = tournament, Form = model });
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, TournamentTransition transition, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(new ChangeTournamentStatusCommand(id, transition), cancellationToken);
        Report(result, localizer["admin.tournament.status_changed"].Value, localizer);

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:guid}/teams")]
    public async Task<IActionResult> AddTeam(Guid id, ParticipantInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!ModelState.IsValid)
        {
            ReportFailure(FirstModelError());
        }
        else
        {
            var result = await sender.Send(new AddTournamentTeamCommand(id, input.TeamId, input.Seed), cancellationToken);
            Report(result, localizer["admin.tournament.team_added"].Value, localizer);
        }

        return RedirectToAction(nameof(Edit), null, new { id }, "participants");
    }

    [HttpPost("{id:guid}/teams/{teamId:guid}")]
    public async Task<IActionResult> UpdateTeam(Guid id, Guid teamId, ParticipantInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!ModelState.IsValid)
        {
            ReportFailure(FirstModelError());
        }
        else
        {
            var result = await sender.Send(
                new UpdateTournamentTeamCommand(id, teamId, input.Seed, input.Placement),
                cancellationToken);
            Report(result, localizer["admin.saved"].Value, localizer);
        }

        return RedirectToAction(nameof(Edit), null, new { id }, "participants");
    }

    [HttpPost("{id:guid}/teams/{teamId:guid}/remove")]
    public async Task<IActionResult> RemoveTeam(Guid id, Guid teamId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveTournamentTeamCommand(id, teamId), cancellationToken);
        Report(result, localizer["admin.tournament.team_removed"].Value, localizer);

        return RedirectToAction(nameof(Edit), null, new { id }, "participants");
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTournamentCommand(id), cancellationToken);
        Report(result, localizer["admin.tournament.deleted"].Value, localizer);

        return result.IsSuccess
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Edit), new { id });
    }
}