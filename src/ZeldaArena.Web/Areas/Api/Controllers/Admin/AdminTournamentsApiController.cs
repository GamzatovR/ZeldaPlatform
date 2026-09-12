using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Tournaments;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Api.Controllers.Admin;

[Route("api/admin/tournaments")]
[Authorize(Policy = PolicyNames.CanManageCatalog)]
public sealed class AdminTournamentsApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminApiControllerBase(localizer)
{
    public const string TablePartial = "~/Areas/Admin/Views/Tournaments/_TournamentTable.cshtml";

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTournamentsForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            TablePartial,
            new TournamentIndexViewModel { Filter = filter, Result = result },
            AdminPageUrl("Index", "Tournaments"));
    }
}