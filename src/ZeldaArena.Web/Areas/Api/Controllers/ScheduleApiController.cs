using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Matches.Queries.GetSchedule;
using ZeldaArena.Web.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers;

[Route("api/schedule")]
public sealed class ScheduleApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetScheduleQuery filter, CancellationToken cancellationToken)
    {
        var schedule = await sender.Send(filter, cancellationToken);

        return ListPartial(
            "~/Views/Schedule/_ScheduleDays.cshtml",
            schedule,
            PageUrl(nameof(ScheduleController.Index), "Schedule"));
    }
}