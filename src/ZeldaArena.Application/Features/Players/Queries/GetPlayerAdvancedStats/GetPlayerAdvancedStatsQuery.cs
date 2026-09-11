using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;

/// <summary>
/// Расширенная статистика игрока — функция <c>stats.advanced</c> (docs/SPEC.md §7.2):
/// разбивка показателей по турнирам. Без функции — отказ сценария, как у команды.
/// </summary>
public sealed record GetPlayerAdvancedStatsQuery(Guid PlayerId) : IQuery<Result<IReadOnlyList<PlayerTournamentStatsDto>>>;