using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Teams.Queries.GetTeams;
using ZeldaArena.Web.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>Фильтр, поиск и пагинация команд.</summary>
[Route("api/teams")]
public sealed class TeamsApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTeamsQuery filter, CancellationToken cancellationToken)
    {
        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            "~/Views/Teams/_TeamList.cshtml",
            result,
            PageUrl(nameof(TeamsController.Index), "Teams"));
    }
}