using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;

/// <summary>
/// Расширенная статистика команды — платная функция <c>stats.advanced</c>
/// (docs/SPEC.md §7.2): средние показатели игроков и разбивка по турнирам.
///
/// Результат, а не просто данные: без функции сценарий отвечает отказом, и данные
/// не попадают в разметку неподписчику, даже если во вьюхе забудут <c>&lt;feature-gate&gt;</c>
/// (§7.3: «сервер перепроверяет всегда»).
/// </summary>
public sealed record GetTeamAdvancedStatsQuery(Guid TeamId) : IQuery<Result<TeamAdvancedStatsDto>>;