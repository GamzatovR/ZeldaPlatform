namespace ZeldaArena.Application.Features.Matches.Queries.GetSchedule;

/// <summary>
/// Страница расписания: матчи страницы, сгруппированные по дням, и данные пагинации.
/// Пагинация идёт по матчам, а не по дням: день с двадцатью матчами не должен
/// превращать одну страницу в бесконечную.
/// </summary>
public sealed record ScheduleDto
{
    public IReadOnlyList<ScheduleDayDto> Days { get; init; } = [];

    public int Page { get; init; }

    public int TotalPages { get; init; }

    public int TotalCount { get; init; }
}