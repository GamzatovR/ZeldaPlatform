using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Matches.Queries.GetSchedule;

public sealed record GetScheduleQuery : FilterBase, IQuery<ScheduleDto>
{
    /// <summary>Слаг турнира. Пусто — все турниры.</summary>
    public string? Tournament { get; init; }

    public MatchStatus? Status { get; init; }
}