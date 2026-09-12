using ZeldaArena.Application.Features.Matches.Queries.GetSchedule;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Matches;

public class GetScheduleQueryHandlerTests
{
    private static readonly DateTimeOffset Today = new(2026, 6, 15, 10, 0, 0, TimeSpan.Zero);

    private readonly EsportsWorld _world = new();
    private readonly Tournament _otherTournament = Tournament.Announce(
        Slug.From("gerudo-cup"),
        "Gerudo Cup",
        TournamentTier.B,
        Region.Europe,
        Money.FromRubles(50_000m),
        Today.AddDays(5),
        Today.AddDays(9));

    public GetScheduleQueryHandlerTests()
    {
        var knights = _world.AddTeam("Hyrule Knights", "HYR");
        var guard = _world.AddTeam("Kakariko Guard", "KAK");

        _world.Schedule(knights, guard, Today.AddHours(2));
        _world.Schedule(knights, guard, Today.AddHours(6));
        _world.Schedule(knights, guard, Today.AddDays(1));
        _world.Play(knights, guard, Today.AddDays(-2), 2, 0);
        _world.Play(knights, guard, Today.AddDays(-1), 0, 2);

        var live = _world.Schedule(knights, guard, Today.AddHours(-1));
        live.Start(Today.AddHours(-1));

        _world.Matches.Add(Match.Schedule(_otherTournament.Id, knights.Id, guard.Id, Today.AddDays(6), bestOf: 3));
    }

    [Fact]
    public async Task By_default_shows_what_is_ahead_including_live()
    {
        var result = await Handle(new GetScheduleQuery());

        result.TotalCount.ShouldBe(5);
        result.Days.SelectMany(day => day.Matches).ShouldAllBe(match =>
            match.Status == MatchStatus.Live || match.Status == MatchStatus.Scheduled);
    }

    [Fact]
    public async Task Matches_are_grouped_by_day_in_chronological_order()
    {
        var result = await Handle(new GetScheduleQuery());

        result.Days.Select(day => day.Day).ShouldBe(
        [
            new DateOnly(2026, 6, 15),
            new DateOnly(2026, 6, 16),
            new DateOnly(2026, 6, 21),
        ]);
        result.Days[0].Matches.Count.ShouldBe(3);
        result.Days[0].Matches[0].IsLive.ShouldBeTrue();
    }

    [Fact]
    public async Task Finished_filter_shows_the_latest_result_first()
    {
        var result = await Handle(new GetScheduleQuery { Status = MatchStatus.Finished });

        result.Days.Select(day => day.Day).ShouldBe([new DateOnly(2026, 6, 14), new DateOnly(2026, 6, 13)]);
    }

    [Fact]
    public async Task Tournament_filter_narrows_by_slug()
    {
        var result = await Handle(new GetScheduleQuery { Tournament = "gerudo-cup" });

        result.Days.ShouldHaveSingleItem().Matches.ShouldHaveSingleItem();
    }

    /// <summary>Неизвестный турнир — пустое расписание, а не молча сброшенный фильтр.</summary>
    [Theory]
    [InlineData("no-such-cup")]
    [InlineData("!!!")]
    public async Task Unknown_tournament_gives_an_empty_schedule(string slug)
    {
        var result = await Handle(new GetScheduleQuery { Tournament = slug });

        result.TotalCount.ShouldBe(0);
        result.Days.ShouldBeEmpty();
    }

    [Fact]
    public async Task Schedule_is_paged_by_matches_not_by_days()
    {
        var result = await Handle(new GetScheduleQuery { Page = 2, PageSize = 12 });

        result.Page.ShouldBe(2);
        result.Days.ShouldBeEmpty();
        result.TotalPages.ShouldBe(1);
    }

    private Task<ScheduleDto> Handle(GetScheduleQuery query)
    {
        var handler = new GetScheduleQueryHandler(
            _world.Read(_world.Matches),
            _world.Read([_world.Tournament, _otherTournament]),
            new InMemoryQueryExecutor());

        return handler.Handle(query, CancellationToken.None);
    }
}