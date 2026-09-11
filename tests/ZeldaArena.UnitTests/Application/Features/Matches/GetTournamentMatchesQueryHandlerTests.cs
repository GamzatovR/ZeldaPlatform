using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Matches;

/// <summary>
/// Вкладки матчей турнира (docs/SPEC.md §9.3, п. 4). Порядок внутри вкладки —
/// часть её смысла: live первыми среди предстоящих, свежие первыми среди прошедших.
/// </summary>
public class GetTournamentMatchesQueryHandlerTests
{
    private static readonly Guid Tournament = Guid.CreateVersion7();
    private static readonly Guid OtherTournament = Guid.CreateVersion7();
    private static readonly Guid TeamA = Guid.CreateVersion7();
    private static readonly Guid TeamB = Guid.CreateVersion7();
    private static readonly DateTimeOffset Day = new(2026, 6, 10, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task All_tab_lists_every_match_of_the_tournament_chronologically()
    {
        var result = await Handle(MatchListState.All);

        result.TotalCount.ShouldBe(6);
        result.Items.Select(match => match.ScheduledAt).ShouldBeInOrder();
    }

    [Fact]
    public async Task Upcoming_tab_puts_live_first_and_includes_postponed()
    {
        var result = await Handle(MatchListState.Upcoming);

        result.Items.Select(match => match.Status)
            .ShouldBe([MatchStatus.Live, MatchStatus.Scheduled, MatchStatus.Postponed]);
    }

    [Fact]
    public async Task Past_tab_shows_the_latest_result_first()
    {
        var result = await Handle(MatchListState.Past);

        result.Items.Select(match => match.Status)
            .ShouldBe([MatchStatus.Canceled, MatchStatus.Finished, MatchStatus.Canceled]);
        result.Items.Select(match => match.ScheduledAt).ShouldBeInOrder(SortDirection.Descending);
    }

    [Fact]
    public async Task Matches_of_other_tournaments_never_leak_in()
    {
        var result = await Handle(MatchListState.All);

        result.Items.ShouldAllBe(match => match.Id != Foreign.Id);
    }

    [Fact]
    public async Task Tab_is_paged()
    {
        var result = await Handle(MatchListState.All, page: 2, pageSize: 12);

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(6);
    }

    [Fact]
    public void Validator_rejects_an_unknown_tab() =>
        new GetTournamentMatchesQueryValidator()
            .Validate(new GetTournamentMatchesQuery { TournamentId = Tournament, State = (MatchListState)42 })
            .IsValid.ShouldBeFalse();

    private static readonly Match Foreign = Match.Schedule(OtherTournament, TeamA, TeamB, Day, bestOf: 3);

    private static Task<PagedResult<MatchCardDto>> Handle(MatchListState state, int page = 1, int pageSize = 12)
    {
        var handler = new GetTournamentMatchesQueryHandler(
            new InMemoryReadRepository<Match>(Matches()),
            new InMemoryQueryExecutor());

        return handler.Handle(
            new GetTournamentMatchesQuery { TournamentId = Tournament, State = state, Page = page, PageSize = pageSize },
            CancellationToken.None);
    }

    private static IReadOnlyList<Match> Matches()
    {
        var finished = Match.Schedule(Tournament, TeamA, TeamB, Day.AddDays(-3), bestOf: 3);
        finished.Start(Day.AddDays(-3));
        finished.UpdateScore(2, 1);
        finished.Finish(Day.AddDays(-3).AddHours(2));

        var canceled = Match.Schedule(Tournament, TeamA, TeamB, Day.AddDays(-1), bestOf: 3);
        canceled.Cancel();

        // Идёт, но запланирован позже запланированного: одной сортировкой по времени
        // он оказался бы вторым.
        var live = Match.Schedule(Tournament, TeamA, TeamB, Day.AddDays(2), bestOf: 3);
        live.Start(Day.AddDays(2));

        var scheduled = Match.Schedule(Tournament, TeamA, TeamB, Day.AddDays(1), bestOf: 3);

        var postponed = Match.Schedule(Tournament, TeamA, TeamB, Day.AddDays(3), bestOf: 3);
        postponed.Postpone(Day.AddDays(5));

        var longAgo = Match.Schedule(Tournament, TeamA, TeamB, Day.AddDays(-10), bestOf: 1);
        longAgo.Cancel();

        return [finished, canceled, live, scheduled, postponed, Foreign, longAgo];
    }
}