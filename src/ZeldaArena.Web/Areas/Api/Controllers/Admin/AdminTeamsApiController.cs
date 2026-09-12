using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Teams.Commands.SetTeamApproval;
using ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Teams;
using ZeldaArena.Web.Areas.Api.Models;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Api.Controllers.Admin;

/// <summary>
/// Таблица команд и одобрение без перезагрузки (docs/SPEC.md §10.1, сценарий 12).
/// После одобрения клиент перерисовывает таблицу событием <c>list:refresh</c>:
/// строка меняет и метку, и набор кнопок, и решает это сервер.
/// </summary>
[Route("api/admin/teams")]
[Authorize(Policy = PolicyNames.CanManageCatalog)]
public sealed class AdminTeamsApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminApiControllerBase(localizer)
{
    public const string TablePartial = "~/Areas/Admin/Views/Teams/_TeamTable.cshtml";

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTeamsForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            TablePartial,
            new TeamIndexViewModel { Filter = filter, Result = result },
            AdminPageUrl(nameof(Index), "Teams"));
    }

    [HttpPost("{id:guid}/approval")]
    public async Task<IActionResult> SetApproval(Guid id, [FromForm] bool isApproved, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetTeamApprovalCommand(id, isApproved), cancellationToken);

        return result.IsFailure
            ? Failure(result.Error)
            : Ok(new MessageResponse(Localizer[isApproved ? "admin.team.approved" : "admin.team.revoked"].Value));
    }
}