using ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;
using ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;

namespace ZeldaArena.Web.Models.Teams;

public sealed class TeamDetailsViewModel
{
    public required TeamDetailsDto Team { get; init; }

    public TeamAdvancedStatsDto? AdvancedStats { get; init; }
}