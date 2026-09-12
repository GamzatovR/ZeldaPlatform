namespace ZeldaArena.Application.Features.Matches.Queries.GetSchedule;

public sealed record ScheduleDto
{
    public IReadOnlyList<ScheduleDayDto> Days { get; init; } = [];

    public int Page { get; init; }

    public int TotalPages { get; init; }

    public int TotalCount { get; init; }
}