using ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;
using ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;

namespace ZeldaArena.Web.Models.Players;

public sealed class PlayerDetailsViewModel
{
    public required PlayerDetailsDto Player { get; init; }

    public IReadOnlyList<PlayerTournamentStatsDto>? AdvancedStats { get; init; }
}