using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Users.Commands.SetUserBlocked;
using ZeldaArena.Application.Features.Admin.Users.Commands.SetUserRoles;
using ZeldaArena.Application.Features.Admin.Users.Queries.GetUsersForAdmin;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Areas.Admin.Models.Users;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

[Route("admin/users")]
[Authorize(Policy = PolicyNames.AdminOnly)]
public sealed class UsersController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetUsersForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        // Роль приходит строкой, и привязка её не проверяет: неизвестное значение —
        // испорченная ссылка, а не пустой фильтр (как ?status=bogus в других таблицах).
        if (!ModelState.IsValid
            || (filter.Role is not null && !RoleNames.All.Contains(filter.Role, StringComparer.OrdinalIgnoreCase)))
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new UserIndexViewModel { Filter = filter, Result = result });
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, [FromForm] IReadOnlyList<string>? roles, string? returnUrl, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetUserRolesCommand(id, roles ?? []), cancellationToken);
        Report(result, localizer["admin.user.roles_saved"].Value, localizer);

        return Back(returnUrl);
    }

    [HttpPost("{id:guid}/blocked")]
    public async Task<IActionResult> SetBlocked(Guid id, bool isBlocked, string? returnUrl, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetUserBlockedCommand(id, isBlocked), cancellationToken);
        Report(result, localizer[isBlocked ? "admin.user.blocked" : "admin.user.unblocked"].Value, localizer);

        return Back(returnUrl);
    }

    /// <summary>Возврат к тому же состоянию таблицы: фильтр и страница живут в адресе.</summary>
    private IActionResult Back(string? returnUrl) =>
        returnUrl is not null && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
}