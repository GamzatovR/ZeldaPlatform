using ZeldaArena.Application.Features.Matches.Queries.GetSchedule;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentOptions;

namespace ZeldaArena.Web.Models.Schedule;

/// <summary>Расписание (docs/SPEC.md §9.3, п. 2): фильтр, дни с матчами и список турниров для фильтра.</summary>
public sealed class ScheduleViewModel
{
    public required GetScheduleQuery Filter { get; init; }

    public required ScheduleDto Schedule { get; init; }

    public required IReadOnlyList<TournamentOptionDto> Tournaments { get; init; }
}