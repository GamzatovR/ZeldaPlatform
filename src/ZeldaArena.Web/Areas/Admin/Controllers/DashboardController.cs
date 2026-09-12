using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

/// <summary>
/// Дашборд <c>/admin</c> (docs/SPEC.md §9.4, п. 1). Открыт и модератору; деловые
/// показатели сценарий отдаёт только администратору.
/// </summary>
[Route("admin")]
public sealed class DashboardController(ISender sender) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await sender.Send(new GetAdminDashboardQuery(), cancellationToken));
}