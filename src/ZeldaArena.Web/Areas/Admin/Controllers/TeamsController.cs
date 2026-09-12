using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Features.Admin.Teams.Commands.AddTeamPlayer;
using ZeldaArena.Application.Features.Admin.Teams.Commands.ChangeTeamPlayerRole;
using ZeldaArena.Application.Features.Admin.Teams.Commands.CreateTeamByAdmin;
using ZeldaArena.Application.Features.Admin.Teams.Commands.DeleteTeam;
using ZeldaArena.Application.Features.Admin.Teams.Commands.RemoveTeamPlayer;
using ZeldaArena.Application.Features.Admin.Teams.Commands.SetTeamApproval;
using ZeldaArena.Application.Features.Admin.Teams.Commands.UpdateTeamByAdmin;
using ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;
using ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Teams;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

/// <summary>
/// Команды — <c>/admin/teams</c> (docs/SPEC.md §9.4, п. 4): таблица со всеми командами,
/// одобрение команд подписчиков, профиль и составы.
/// </summary>
[Route("admin/teams")]
[Authorize(Policy = PolicyNames.CanManageCatalog)]
public sealed class TeamsController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTeamsForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new TeamIndexViewModel { Filter = filter, Result = result });
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new TeamFormViewModel());

    [HttpPost("create")]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> Create(TeamFormViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var logo = model.Logo?.OpenReadStream();

        var result = await sender.Send(
            new CreateTeamByAdminCommand
            {
                Name = model.Name,
                Tag = model.Tag,
                CountryCode = model.CountryCode,
                Region = model.Region,
                FoundedAt = model.FoundedAt,
                Description = model.Description,
                Rating = model.Rating,
                Logo = model.Logo.ToFileUpload(logo),
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return View(model);
        }

        ReportSuccess(localizer["admin.team.created"].Value);

        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var team = await sender.Send(new GetTeamForEditQuery(id), cancellationToken);

        return team is null
            ? NotFound()
            : View(new TeamEditViewModel { Team = team, Form = TeamFormViewModel.From(team) });
    }

    [HttpPost("{id:guid}")]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> Edit(
        Guid id,
        [Bind(Prefix = nameof(TeamEditViewModel.Form))] TeamFormViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (ModelState.IsValid)
        {
            await using var logo = model.Logo?.OpenReadStream();

            var result = await sender.Send(
                new UpdateTeamByAdminCommand
                {
                    Id = id,
                    Name = model.Name,
                    Tag = model.Tag,
                    CountryCode = model.CountryCode,
                    Region = model.Region,
                    FoundedAt = model.FoundedAt,
                    Description = model.Description,
                    Rating = model.Rating,
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

        var team = await sender.Send(new GetTeamForEditQuery(id), cancellationToken);

        if (team is null)
        {
            return NotFound();
        }

        model.CurrentLogoPath = team.LogoPath;

        return View(new TeamEditViewModel { Team = team, Form = model });
    }

    /// <summary>
    /// Одобрение и снятие одобрения. Та же форма уходит в <c>/api/admin/teams</c>,
    /// когда есть JavaScript, — тогда таблица перерисовывается без перезагрузки.
    /// </summary>
    [HttpPost("{id:guid}/approval")]
    public async Task<IActionResult> SetApproval(Guid id, bool isApproved, string? returnUrl, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetTeamApprovalCommand(id, isApproved), cancellationToken);

        Report(
            result,
            localizer[isApproved ? "admin.team.approved" : "admin.team.revoked"].Value,
            localizer);

        return returnUrl is not null && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:guid}/players")]
    public async Task<IActionResult> AddPlayer(Guid id, RosterInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!ModelState.IsValid)
        {
            ReportFailure(FirstModelError());
        }
        else
        {
            var result = await sender.Send(new AddTeamPlayerCommand(id, input.PlayerId, input.Role), cancellationToken);
            Report(result, localizer["admin.team.player_added"].Value, localizer);
        }

        return RedirectToAction(nameof(Edit), null, new { id }, "roster");
    }

    [HttpPost("{id:guid}/players/{playerId:guid}")]
    public async Task<IActionResult> ChangeRole(Guid id, Guid playerId, RosterInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!ModelState.IsValid)
        {
            ReportFailure(FirstModelError());
        }
        else
        {
            var result = await sender.Send(new ChangeTeamPlayerRoleCommand(id, playerId, input.Role), cancellationToken);
            Report(result, localizer["admin.saved"].Value, localizer);
        }

        return RedirectToAction(nameof(Edit), null, new { id }, "roster");
    }

    [HttpPost("{id:guid}/players/{playerId:guid}/remove")]
    public async Task<IActionResult> RemovePlayer(Guid id, Guid playerId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveTeamPlayerCommand(id, playerId), cancellationToken);
        Report(result, localizer["admin.team.player_removed"].Value, localizer);

        return RedirectToAction(nameof(Edit), null, new { id }, "roster");
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTeamCommand(id), cancellationToken);
        Report(result, localizer["admin.team.deleted"].Value, localizer);

        return result.IsSuccess
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Edit), new { id });
    }
}