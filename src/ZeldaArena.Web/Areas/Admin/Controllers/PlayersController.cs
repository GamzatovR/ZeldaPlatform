using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Features.Admin.Players.Commands.CreatePlayer;
using ZeldaArena.Application.Features.Admin.Players.Commands.DeletePlayer;
using ZeldaArena.Application.Features.Admin.Players.Commands.UpdatePlayer;
using ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayerForEdit;
using ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayersForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Players;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

/// <summary>Игроки — <c>/admin/players</c>.</summary>
[Route("admin/players")]
[Authorize(Policy = PolicyNames.CanManageCatalog)]
public sealed class PlayersController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetPlayersForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new PlayerIndexViewModel { Filter = filter, Result = result });
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new PlayerFormViewModel());

    [HttpPost("create")]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> Create(PlayerFormViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var avatar = model.Avatar?.OpenReadStream();

        var result = await sender.Send(
            new CreatePlayerCommand
            {
                Nickname = model.Nickname,
                CountryCode = model.CountryCode,
                Role = model.Role,
                FirstName = model.FirstName,
                LastName = model.LastName,
                BirthDate = model.BirthDate,
                Bio = model.Bio,
                Avatar = model.Avatar.ToFileUpload(avatar),
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return View(model);
        }

        ReportSuccess(localizer["admin.player.created"].Value);

        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var player = await sender.Send(new GetPlayerForEditQuery(id), cancellationToken);

        return player is null
            ? NotFound()
            : View(new PlayerEditViewModel { Player = player, Form = PlayerFormViewModel.From(player) });
    }

    [HttpPost("{id:guid}")]
    [RequestSizeLimit(ImageUploadRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadRules.MaxRequestBytes)]
    public async Task<IActionResult> Edit(
        Guid id,
        [Bind(Prefix = nameof(PlayerEditViewModel.Form))] PlayerFormViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (ModelState.IsValid)
        {
            await using var avatar = model.Avatar?.OpenReadStream();

            var result = await sender.Send(
                new UpdatePlayerCommand
                {
                    Id = id,
                    Nickname = model.Nickname,
                    CountryCode = model.CountryCode,
                    Role = model.Role,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    Bio = model.Bio,
                    Avatar = model.Avatar.ToFileUpload(avatar),
                    RemoveAvatar = model.RemoveAvatar,
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                ReportSuccess(localizer["admin.saved"].Value);

                return RedirectToAction(nameof(Edit), new { id });
            }

            ModelState.AddResultError(result, localizer);
        }

        var player = await sender.Send(new GetPlayerForEditQuery(id), cancellationToken);

        if (player is null)
        {
            return NotFound();
        }

        model.CurrentAvatarPath = player.AvatarPath;

        return View(new PlayerEditViewModel { Player = player, Form = model });
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeletePlayerCommand(id), cancellationToken);
        Report(result, localizer["admin.player.deleted"].Value, localizer);

        return result.IsSuccess
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Edit), new { id });
    }
}