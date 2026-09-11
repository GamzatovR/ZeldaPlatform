using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Matches;

/// <summary>
/// Хендлер проверяется на настоящей коллекции: собранное им выражение исполняется
/// LINQ-провайдером в памяти, поэтому порядок и отбор проверяются по результату,
/// а не по факту вызова подменённого порта — так же, как у списка турниров.
///
/// Названия и логотипы команд остаются пустыми: <c>Match.TeamA</c> и <c>Match.TeamB</c> —
/// навигационные свойства без публичного сеттера, их заполняет EF Core при загрузке,
/// а подставить их здесь можно было бы только рефлексией — то есть проверялась бы
/// рефлексия, а не хендлер. Зато сам факт, что выражение исполняется вне EF Core
/// и не падает, — это и есть проверка: первая версия проекции разворачивала
/// <c>TeamA.Name</c> без защиты от null и роняла все четыре теста.
/// Что проекция действительно уходит в JOIN, проверяется на живой базе
/// (запись Фазы 5 в docs/PROGRESS.md).
/// </summary>
public class GetHomeMatchesQueryHandlerTests
{
    private static readonly Guid Tournament = Guid.NewGuid();
    private static readonly Guid Fierce = Guid.NewGuid();
    private static readonly Guid Devlis = Guid.NewGuid();
    private static readonly DateTimeOffset Season = new(2026, 6, 1, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Live_matches_come_before_scheduled_ones()
    {
        var result = await Handle(new GetHomeMatchesQuery());

        result.First().IsLive.ShouldBeTrue();
    }

    /// <summary>
    /// Идущий матч запланирован позже всех остальных, поэтому одной сортировкой
    /// по времени он оказался бы последним. Проверка именно про это: приоритет
    /// статуса сильнее времени.
    /// </summary>
    [Fact]
    public async Task Within_a_group_matches_are_ordered_by_schedule()
    {
        var result = await Handle(new GetHomeMatchesQuery(Count: 12));

        result.Select(match => match.ScheduledAt)
            .ShouldBe([Season.AddDays(9), Season.AddDays(1), Season.AddDays(3), Season.AddDays(5)]);
    }

    [Fact]
    public async Task Finished_and_cancelled_matches_never_reach_the_home_page()
    {
        var result = await Handle(new GetHomeMatchesQuery(Count: 12));

        result.ShouldAllBe(match =>
            match.Status == MatchStatus.Live || match.Status == MatchStatus.Scheduled);
    }

    [Fact]
    public async Task Count_limits_the_result()
    {
        var result = await Handle(new GetHomeMatchesQuery(Count: 2));

        result.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(GetHomeMatchesQueryValidator.MaxCount + 1)]
    public void Validator_rejects_a_count_outside_the_allowed_range(int count)
    {
        var result = new GetHomeMatchesQueryValidator().Validate(new GetHomeMatchesQuery(count));

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validator_accepts_the_default_count() =>
        new GetHomeMatchesQueryValidator()
            .Validate(new GetHomeMatchesQuery())
            .IsValid.ShouldBeTrue();

    private static async Task<IReadOnlyList<MatchCardDto>> Handle(GetHomeMatchesQuery query)
    {
        var handler = new GetHomeMatchesQueryHandler(
            new InMemoryReadRepository<Match>(Schedule()),
            new InMemoryQueryExecutor());

        return await handler.Handle(query, CancellationToken.None);
    }

    private static IReadOnlyList<Match> Schedule()
    {
        var soon = Match.Schedule(Tournament, Fierce, Devlis, Season.AddDays(1), bestOf: 3);
        var later = Match.Schedule(Tournament, Fierce, Devlis, Season.AddDays(3), bestOf: 3);
        var latest = Match.Schedule(Tournament, Fierce, Devlis, Season.AddDays(5), bestOf: 3);

        var finished = Match.Schedule(Tournament, Fierce, Devlis, Season.AddDays(-2), bestOf: 3);
        finished.Start(Season.AddDays(-2));
        finished.UpdateScore(2, 0);
        finished.Finish(Season.AddDays(-2).AddHours(2));

        // Идёт прямо сейчас, но запланирован позже всех: одной сортировкой по времени
        // он оказался бы в конце списка.
        var live = Match.Schedule(Tournament, Fierce, Devlis, Season.AddDays(9), bestOf: 3);
        live.Start(Season.AddDays(9));

        return [soon, later, latest, finished, live];
    }
}