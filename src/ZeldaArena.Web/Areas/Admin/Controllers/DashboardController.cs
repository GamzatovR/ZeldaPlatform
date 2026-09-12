using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

[Route("admin")]
public sealed class DashboardController(ISender sender) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await sender.Send(new GetAdminDashboardQuery(), cancellationToken));
}