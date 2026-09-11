using ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;

namespace ZeldaArena.Web.Models.MyTeam;

/// <summary>
/// Кабинет капитана (docs/SPEC.md §9.3, п. 8). <see cref="Team"/> пуст, если своей команды
/// у пользователя нет: страница предлагает создать её.
/// </summary>
public sealed class MyTeamViewModel
{
    public MyTeamDto? Team { get; init; }
}