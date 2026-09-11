using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Matches.Queries.GetSchedule;

/// <summary>
/// Расписание матчей (docs/SPEC.md §9.3, п. 2): группировка по дням, фильтр по турниру
/// и статусу, отметка live. Поля повторяют query-string:
/// <c>/schedule?tournament=hyrule-open-2026&amp;status=finished&amp;page=2</c>.
/// </summary>
public sealed record GetScheduleQuery : FilterBase, IQuery<ScheduleDto>
{
    /// <summary>Слаг турнира. Пусто — все турниры.</summary>
    public string? Tournament { get; init; }

    /// <summary>
    /// Статус матча. Пусто — «впереди»: идущие, запланированные и перенесённые.
    /// Расписание в первую очередь о том, что будет, а прошедшее открывается фильтром.
    /// </summary>
    public MatchStatus? Status { get; init; }
}