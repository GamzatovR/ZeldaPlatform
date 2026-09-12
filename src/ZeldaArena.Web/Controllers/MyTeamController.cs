using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Features.Teams.Commands.AddFreeAgentToMyTeam;
using ZeldaArena.Application.Features.Teams.Commands.AddNewPlayerToMyTeam;
using ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamLogo;
using ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamPlayerRole;
using ZeldaArena.Application.Features.Teams.Commands.RemovePlayerFromMyTeam;
using ZeldaArena.Application.Features.Teams.Commands.UpdateMyTeamProfile;
using ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;
using ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.MyTeam;
using ZeldaArena.Web.Models.Teams;
using ZeldaArena.Web.Validation;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Кабинет капитана <c>/account/my-team</c> (docs/SPEC.md §9.3, п. 8).
///
/// Страница открыта любому вошедшему: после истечения подписки команда остаётся
/// и её видно, но изменяющие действия закрыты функцией <c>team.create</c> — атрибутом
/// здесь и проверкой в каждом сценарии (docs/CONVENTIONS.md, «При неопределённости»; §7.3).
///
/// На странице несколько форм, поэтому каждое действие отвечает по схеме PRG:
/// исход — сообщением в TempData, возврат — на кабинет. Серверная валидация от этого
/// не слабеет: форма без JavaScript отвергается с тем же сообщением (§15, §19).
/// </summary>
[Authorize]
[Route("account/my-team")]
public sealed class MyTeamController(ISender sender, IStringLocalizer<SharedResource> localizer) : Controller
{
    private const string StatusKey = TempDataKeys.StatusMessage;

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid? team, CancellationToken cancellationToken)
    {
        var myTeam = await sender.Send(new GetMyTeamQuery(team), cancellationToken);

        if (myTeam is null || !myTeam.CanEdit)
        {
            return View(new MyTeamViewModel { Team = myTeam });
        }

        var freeAgents = await sender.Send(new GetFreeAgentsQuery(), cancellationToken);

        return View(new MyTeamViewModel
        {
            Team = myTeam,
            FreeAgents = freeAgents,
            Profile = new TeamProfileViewModel
            {
                Name = myTeam.Name,
                Tag = myTeam.Tag,
                CountryCode = myTeam.CountryCode,
                Region = myTeam.Region,
                FoundedAt = myTeam.FoundedAt,
                Description = myTeam.Description,
            },
        });
    }

    [HttpPost("{teamId:guid}/profile")]
    [RequireFeature(FeatureCodes.TeamCreate)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        Guid teamId,
        [Bind(Prefix = nameof(MyTeamViewModel.Profile))] TeamProfileViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return BackWithInvalidInput(teamId);
        }

        var result = await sender.Send(
            new UpdateMyTeamProfileCommand
            {
                TeamId = teamId,
                Name = model.Name,
                Tag = model.Tag,
                CountryCode = model.CountryCode,
                Region = model.Region,
                FoundedAt = model.FoundedAt,
                Description = model.Description,
            },
            cancellationToken);

        return Back(teamId, result, "my_team.profile_saved");
    }

    [HttpPost("{teamId:guid}/logo")]
    [RequireFeature(FeatureCodes.TeamCreate)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> ChangeLogo(
        Guid teamId,
        [ImageFile] IFormFile? logo,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || logo is null)
        {
            return BackWithInvalidInput(teamId);
        }

        await using var content = logo.OpenReadStream();

        var result = await sender.Send(
            new ChangeMyTeamLogoCommand(teamId, logo.ToFileUpload(content)!),
            cancellationToken);

        return Back(teamId, result, "my_team.logo_saved");
    }

    [HttpPost("{teamId:guid}/players")]
    [RequireFeature(FeatureCodes.TeamCreate)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNewPlayer(
        Guid teamId,
        [Bind(Prefix = nameof(MyTeamViewModel.NewPlayer))] NewPlayerViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return BackWithInvalidInput(teamId);
        }

        var result = await sender.Send(
            new AddNewPlayerToMyTeamCommand
            {
                TeamId = teamId,
                Nickname = model.Nickname,
                Role = model.Role,
                CountryCode = model.CountryCode,
                FirstName = model.FirstName,
                LastName = model.LastName,
            },
            cancellationToken);

        return Back(teamId, result, "my_team.player_added");
    }

    [HttpPost("{teamId:guid}/players/free-agent")]
    [RequireFeature(FeatureCodes.TeamCreate)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFreeAgent(
        Guid teamId,
        Guid playerId,
        PlayerRole role,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BackWithInvalidInput(teamId);
        }

        var result = await sender.Send(new AddFreeAgentToMyTeamCommand(teamId, playerId, role), cancellationToken);

        return Back(teamId, result, "my_team.player_added");
    }

    [HttpPost("{teamId:guid}/players/{playerId:guid}/role")]
    [RequireFeature(FeatureCodes.TeamCreate)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(
        Guid teamId,
        Guid playerId,
        PlayerRole role,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BackWithInvalidInput(teamId);
        }

        var result = await sender.Send(new ChangeMyTeamPlayerRoleCommand(teamId, playerId, role), cancellationToken);

        return Back(teamId, result, "my_team.role_saved");
    }

    [HttpPost("{teamId:guid}/players/{playerId:guid}/remove")]
    [RequireFeature(FeatureCodes.TeamCreate)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePlayer(Guid teamId, Guid playerId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemovePlayerFromMyTeamCommand(teamId, playerId), cancellationToken);

        return Back(teamId, result, "my_team.player_removed");
    }

    /// <summary>
    /// Исход действия — в TempData и обратно в кабинет. Ошибка помечается ведущим «!»,
    /// как заведено в <c>_StatusMessage</c> с Фазы 3.
    /// </summary>
    private RedirectToActionResult Back(Guid teamId, Result result, string successKey)
    {
        TempData[StatusKey] = result.IsSuccess
            ? localizer[successKey].Value
            : "!" + localizer[result.Error.Code].Value;

        return RedirectToAction(nameof(Index), new { team = teamId });
    }

    /// <summary>
    /// Форма, не прошедшая проверку модели, — без JavaScript или в обход клиентской
    /// валидации. В кабинет возвращается первое сообщение: страница с несколькими формами
    /// не может перерисовать одну из них с ошибками, не потеряв остальные.
    /// </summary>
    private RedirectToActionResult BackWithInvalidInput(Guid teamId)
    {
        var message = ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
            ?? localizer["my_team.invalid_input"].Value;

        TempData[StatusKey] = "!" + message;

        return RedirectToAction(nameof(Index), new { team = teamId });
    }
}