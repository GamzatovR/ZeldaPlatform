using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.UnitTests.Domain.Esports;

public class MatchTests
{
    private static readonly Guid TournamentId = Guid.CreateVersion7();
    private static readonly Guid TeamAId = Guid.CreateVersion7();
    private static readonly Guid TeamBId = Guid.CreateVersion7();
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Scheduled_match_starts_with_zero_score()
    {
        var match = ScheduleBo3();

        match.Status.ShouldBe(MatchStatus.Scheduled);
        match.ScoreA.ShouldBe(0);
        match.ScoreB.ShouldBe(0);
        match.WinnerTeamId.ShouldBeNull();
        match.WinsRequired.ShouldBe(2);
    }

    [Fact]
    public void Team_cannot_play_against_itself()
    {
        Should.Throw<InvariantViolationException>(
                () => Match.Schedule(TournamentId, TeamAId, TeamAId, Now, 3))
            .Code.ShouldBe("match.same_team");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void Best_of_must_be_odd_and_positive(int bestOf)
    {
        Should.Throw<InvariantViolationException>(
                () => Match.Schedule(TournamentId, TeamAId, TeamBId, Now, bestOf))
            .Code.ShouldBe("match.invalid_best_of");
    }

    [Fact]
    public void Score_of_scheduled_match_cannot_be_changed()
    {
        var match = ScheduleBo3();

        Should.Throw<InvariantViolationException>(() => match.UpdateScore(1, 0))
            .Code.ShouldBe("match.not_live");
    }

    [Fact]
    public void Live_match_accepts_score_and_raises_event()
    {
        var match = LiveBo3();

        match.UpdateScore(1, 0);

        match.ScoreA.ShouldBe(1);
        match.DomainEvents.OfType<MatchScoreChangedEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Repeated_identical_score_does_not_raise_event()
    {
        // Пульт модератора может прислать тот же счёт дважды — зрителям это лишний шум.
        var match = LiveBo3();
        match.UpdateScore(1, 0);
        match.ClearDomainEvents();

        match.UpdateScore(1, 0);

        match.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Score_cannot_exceed_wins_required()
    {
        // В Bo3 победа — это две карты, счёт 3:0 невозможен (docs/SPEC.md §5.3).
        var match = LiveBo3();

        Should.Throw<InvariantViolationException>(() => match.UpdateScore(3, 0))
            .Code.ShouldBe("match.score_exceeds_wins_required");
    }

    [Fact]
    public void Sum_of_scores_cannot_exceed_best_of()
    {
        var match = Match.Schedule(TournamentId, TeamAId, TeamBId, Now, 5);
        match.Start(Now);

        Should.Throw<InvariantViolationException>(() => match.UpdateScore(3, 3))
            .Code.ShouldBe("match.score_exceeds_best_of");
    }

    [Fact]
    public void Both_teams_cannot_reach_winning_score()
    {
        var match = LiveBo3();

        Should.Throw<InvariantViolationException>(() => match.UpdateScore(2, 2))
            .Code.ShouldBe("match.score_exceeds_best_of");
    }

    [Fact]
    public void Negative_score_is_rejected()
    {
        var match = LiveBo3();

        Should.Throw<InvariantViolationException>(() => match.UpdateScore(-1, 0))
            .Code.ShouldBe("match.negative_score");
    }

    [Fact]
    public void Finished_match_is_read_only()
    {
        var match = FinishedBo3();

        Should.Throw<InvariantViolationException>(() => match.UpdateScore(2, 1))
            .Code.ShouldBe("match.finished_is_read_only");
    }

    [Fact]
    public void Finish_derives_winner_from_score()
    {
        var match = FinishedBo3();

        match.Status.ShouldBe(MatchStatus.Finished);
        match.WinnerTeamId.ShouldBe(TeamAId);
        match.EndedAt.ShouldNotBeNull();
        match.DomainEvents.OfType<MatchFinishedEvent>().ShouldHaveSingleItem()
            .WinnerTeamId.ShouldBe(TeamAId);
    }

    [Fact]
    public void Match_without_winner_cannot_be_finished()
    {
        var match = LiveBo3();
        match.UpdateScore(1, 1);

        Should.Throw<InvariantViolationException>(() => match.Finish(Now))
            .Code.ShouldBe("match.no_winner_yet");
    }

    [Fact]
    public void Finished_match_cannot_be_canceled()
    {
        var match = FinishedBo3();

        Should.Throw<InvariantViolationException>(match.Cancel)
            .Code.ShouldBe("match.finished_is_read_only");
    }

    [Fact]
    public void Postponed_match_can_still_start()
    {
        var match = ScheduleBo3();
        match.Postpone(Now.AddDays(1));

        match.Status.ShouldBe(MatchStatus.Postponed);

        match.Start(Now.AddDays(1));

        match.Status.ShouldBe(MatchStatus.Live);
    }

    [Fact]
    public void Player_stats_are_limited_to_participating_teams()
    {
        var match = LiveBo3();

        Should.Throw<InvariantViolationException>(
                () => match.AddPlayerStats(Guid.CreateVersion7(), Guid.CreateVersion7(), 10, 5, 3, 2400, 1.15m))
            .Code.ShouldBe("match.foreign_team");
    }

    [Fact]
    public void Player_stats_are_recorded_once()
    {
        var match = LiveBo3();
        var playerId = Guid.CreateVersion7();
        match.AddPlayerStats(playerId, TeamAId, 10, 5, 3, 2400, 1.15m);

        Should.Throw<InvariantViolationException>(
                () => match.AddPlayerStats(playerId, TeamAId, 1, 1, 1, 100, 0.5m))
            .Code.ShouldBe("match.duplicate_player_stats");

        match.PlayerStats.ShouldHaveSingleItem();
    }

    private static Match ScheduleBo3() =>
        Match.Schedule(TournamentId, TeamAId, TeamBId, Now, 3);

    private static Match LiveBo3()
    {
        var match = ScheduleBo3();
        match.Start(Now);
        match.ClearDomainEvents();
        return match;
    }

    private static Match FinishedBo3()
    {
        var match = LiveBo3();
        match.UpdateScore(2, 1);
        match.Finish(Now.AddHours(1));
        return match;
    }
}