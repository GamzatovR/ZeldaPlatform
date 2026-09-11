using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;
using ZeldaArena.Web.Models.MyTeam;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Кабинет капитана <c>/account/my-team</c> (docs/SPEC.md §9.3, п. 8).
///
/// Страница открыта любому вошедшему: после истечения подписки команда остаётся
/// и её видно, но изменяющие действия закрыты функцией <c>team.create</c> — атрибутом
/// здесь и проверкой в каждом сценарии (CLAUDE.md, «При неопределённости»; §7.3).
/// </summary>
[Authorize]
[Route("account/my-team")]
public sealed class MyTeamController(ISender sender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(Guid? team, CancellationToken cancellationToken)
    {
        var myTeam = await sender.Send(new GetMyTeamQuery(team), cancellationToken);

        return View(new MyTeamViewModel { Team = myTeam });
    }
}