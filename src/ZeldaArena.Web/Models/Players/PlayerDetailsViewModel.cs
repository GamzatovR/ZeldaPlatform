using ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;
using ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;

namespace ZeldaArena.Web.Models.Players;

/// <summary>
/// Страница игрока (docs/SPEC.md §9.3, п. 10). <see cref="AdvancedStats"/> пуст без функции
/// <c>stats.advanced</c>: данных нет в разметке, а не просто спрятаны.
/// </summary>
public sealed class PlayerDetailsViewModel
{
    public required PlayerDetailsDto Player { get; init; }

    public IReadOnlyList<PlayerTournamentStatsDto>? AdvancedStats { get; init; }
}