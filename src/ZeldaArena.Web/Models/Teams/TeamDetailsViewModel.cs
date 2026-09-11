using ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;
using ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;

namespace ZeldaArena.Web.Models.Teams;

/// <summary>
/// Страница команды (docs/SPEC.md §9.3, п. 7). <see cref="AdvancedStats"/> пуст,
/// когда у зрителя нет функции <c>stats.advanced</c>: сценарий отказал, и данных
/// в разметке нет вовсе, а не просто спрятаны.
/// </summary>
public sealed class TeamDetailsViewModel
{
    public required TeamDetailsDto Team { get; init; }

    public TeamAdvancedStatsDto? AdvancedStats { get; init; }
}