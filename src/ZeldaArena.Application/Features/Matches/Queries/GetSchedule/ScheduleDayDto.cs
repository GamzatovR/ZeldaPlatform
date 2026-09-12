using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetSchedule;

public sealed record ScheduleDayDto(DateOnly Day, IReadOnlyList<MatchCardDto> Matches);