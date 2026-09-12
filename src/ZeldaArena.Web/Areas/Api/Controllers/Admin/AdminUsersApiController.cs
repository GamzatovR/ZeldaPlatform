using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Users.Commands.SetUserBlocked;
using ZeldaArena.Application.Features.Admin.Users.Queries.GetUsersForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Users;
using ZeldaArena.Web.Areas.Api.Models;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Api.Controllers.Admin;

/// <summary>Таблица пользователей и блокировка без перезагрузки (§10.1, сценарий 12).</summary>
[Route("api/admin/users")]
[Authorize(Policy = PolicyNames.AdminOnly)]
public sealed class AdminUsersApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminApiControllerBase(localizer)
{
    public const string TablePartial = "~/Areas/Admin/Views/Users/_UserTable.cshtml";

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetUsersForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            TablePartial,
            new UserIndexViewModel { Filter = filter, Result = result },
            AdminPageUrl(nameof(Index), "Users"));
    }

    [HttpPost("{id:guid}/blocked")]
    public async Task<IActionResult> SetBlocked(Guid id, [FromForm] bool isBlocked, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetUserBlockedCommand(id, isBlocked), cancellationToken);

        return result.IsFailure
            ? Failure(result.Error)
            : Ok(new MessageResponse(Localizer[isBlocked ? "admin.user.blocked" : "admin.user.unblocked"].Value));
    }
}