using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayersForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Players;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Api.Controllers.Admin;

[Route("api/admin/players")]
[Authorize(Policy = PolicyNames.CanManageCatalog)]
public sealed class AdminPlayersApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminApiControllerBase(localizer)
{
    public const string TablePartial = "~/Areas/Admin/Views/Players/_PlayerTable.cshtml";

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetPlayersForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            TablePartial,
            new PlayerIndexViewModel { Filter = filter, Result = result },
            AdminPageUrl(nameof(Index), "Players"));
    }
}