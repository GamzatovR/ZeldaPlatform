using ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;
using ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;
using ZeldaArena.Web.Models.Teams;

namespace ZeldaArena.Web.Models.MyTeam;

public sealed class MyTeamViewModel
{
    public MyTeamDto? Team { get; init; }

    public TeamProfileViewModel? Profile { get; init; }

    public IReadOnlyList<FreeAgentDto> FreeAgents { get; init; } = [];

    public NewPlayerViewModel NewPlayer { get; init; } = new();
}