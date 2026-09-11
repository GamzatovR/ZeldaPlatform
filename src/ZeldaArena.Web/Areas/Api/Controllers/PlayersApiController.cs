using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Players.Queries.GetPlayers;
using ZeldaArena.Web.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>Фильтр, поиск и пагинация игроков (docs/SPEC.md §10.1, сценарии 1 и 3).</summary>
[Route("api/players")]
public sealed class PlayersApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetPlayersQuery filter, CancellationToken cancellationToken)
    {
        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            "~/Views/Players/_PlayerList.cshtml",
            result,
            PageUrl(nameof(PlayersController.Index), "Players"));
    }
}
