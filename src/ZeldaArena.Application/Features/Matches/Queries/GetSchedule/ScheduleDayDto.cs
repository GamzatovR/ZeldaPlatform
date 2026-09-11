using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetSchedule;

/// <summary>
/// День расписания. Граница дня — по UTC: время хранится в UTC (docs/SPEC.md §9.5),
/// а часового пояса зрителя сервер не знает.
/// </summary>
public sealed record ScheduleDayDto(DateOnly Day, IReadOnlyList<MatchCardDto> Matches);