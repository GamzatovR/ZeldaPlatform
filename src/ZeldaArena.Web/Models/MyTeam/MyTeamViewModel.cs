using ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;
using ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;
using ZeldaArena.Web.Models.Teams;

namespace ZeldaArena.Web.Models.MyTeam;

/// <summary>
/// Кабинет капитана (docs/SPEC.md §9.3, п. 8). <see cref="Team"/> пуст, если своей команды
/// у пользователя нет: страница предлагает создать её. Формы правки заполняются только
/// тогда, когда править можно, — без функции страница остаётся только для чтения.
/// </summary>
public sealed class MyTeamViewModel
{
    public MyTeamDto? Team { get; init; }

    public TeamProfileViewModel? Profile { get; init; }

    public IReadOnlyList<FreeAgentDto> FreeAgents { get; init; } = [];

    public NewPlayerViewModel NewPlayer { get; init; } = new();
}