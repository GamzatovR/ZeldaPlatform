using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Admin.Matches.Commands.ChangeMatchStatus;
using ZeldaArena.Application.Features.Admin.Matches.Commands.DeleteMatch;
using ZeldaArena.Application.Features.Admin.Matches.Commands.ScheduleMatch;
using ZeldaArena.Application.Features.Admin.Matches.Commands.UpdateMatch;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Admin.Matches;

public class MatchCommandTests
{
    private static readonly DateTimeOffset Now = EsportsWorld.Now;

    private readonly EsportsWorld _world = new();
    private readonly RecordingUnitOfWork _unitOfWork = new();
    private readonly InMemoryRepository<Match> _matches = new();
    private readonly Team _knights;
    private readonly Team _gerudo;

    public MatchCommandTests()
    {
        _knights = _world.AddTeam("Hyrule Knights", "HYR");
        _gerudo = _world.AddTeam("Gerudo Valley", "GRD");
        _world.Tournament.AddTeam(_knights.Id, 1);
        _world.Tournament.AddTeam(_gerudo.Id, 2);
    }

    [Fact]
    public async Task Match_between_participants_is_scheduled()
    {
        var result = await Schedule(_knights.Id, _gerudo.Id);

        result.IsSuccess.ShouldBeTrue();
        var match = _matches.Entities.ShouldHaveSingleItem();
        match.TournamentId.ShouldBe(_world.Tournament.Id);
        match.Status.ShouldBe(MatchStatus.Scheduled);
        _unitOfWork.SaveChangesCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Team_outside_the_tournament_cannot_play_in_it()
    {
        var outsider = _world.AddTeam("Lost Woods", "LWD");

        var result = await Schedule(_knights.Id, outsider.Id);

        result.Error.ShouldBe(EsportsErrors.TeamNotInTournament);
        _matches.Entities.ShouldBeEmpty();
    }

    [Fact]
    public async Task Finished_tournament_takes_no_new_matches()
    {
        _world.Tournament.Start();
        _world.Tournament.Finish();

        var result = await Schedule(_knights.Id, _gerudo.Id);

        result.Error.Code.ShouldBe("tournament.roster_is_closed");
    }

    [Fact]
    public async Task Team_cannot_play_itself_and_the_entity_says_so()
    {
        var result = await Schedule(_knights.Id, _knights.Id);

        result.Error.Code.ShouldBe("match.same_team");
        _matches.Entities.ShouldBeEmpty();
    }

    [Fact]
    public async Task Start_finish_and_cancel_go_through_the_entity()
    {
        var match = ScheduleDirectly();
        var handler = new ChangeMatchStatusCommandHandler(_matches, new FixedDateTimeProvider(Now), _unitOfWork);

        var earlyFinish = await handler.Handle(new ChangeMatchStatusCommand(match.Id, MatchTransition.Finish), CancellationToken.None);
        var start = await handler.Handle(new ChangeMatchStatusCommand(match.Id, MatchTransition.Start), CancellationToken.None);

        earlyFinish.Error.Code.ShouldBe("match.not_live");
        start.IsSuccess.ShouldBeTrue();
        match.Status.ShouldBe(MatchStatus.Live);
        match.StartedAt.ShouldBe(Now, "время старта берётся с часов приложения, а не из запроса");
    }

    [Fact]
    public async Task Match_without_a_winner_is_not_finished()
    {
        var match = ScheduleDirectly();
        match.Start(Now);
        match.UpdateScore(1, 0);
        var handler = new ChangeMatchStatusCommandHandler(_matches, new FixedDateTimeProvider(Now), _unitOfWork);

        var result = await handler.Handle(new ChangeMatchStatusCommand(match.Id, MatchTransition.Finish), CancellationToken.None);

        result.Error.Code.ShouldBe("match.no_winner_yet");
        match.Status.ShouldBe(MatchStatus.Live);
    }

    [Fact]
    public async Task Postponed_match_keeps_its_status_when_the_time_moves_again()
    {
        var match = ScheduleDirectly();
        match.Postpone(Now.AddDays(1));

        var result = await new UpdateMatchCommandHandler(_matches, _unitOfWork)
            .Handle(new UpdateMatchCommand(match.Id, Now.AddDays(2), "https://twitch.tv/zelda"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        match.Status.ShouldBe(MatchStatus.Postponed);
        match.ScheduledAt.ShouldBe(Now.AddDays(2));
        match.StreamUrl.ShouldBe("https://twitch.tv/zelda");
    }

    [Fact]
    public async Task Stream_of_a_live_match_changes_without_touching_the_time()
    {
        var match = ScheduleDirectly();
        match.Start(Now);

        var result = await new UpdateMatchCommandHandler(_matches, _unitOfWork)
            .Handle(new UpdateMatchCommand(match.Id, match.ScheduledAt, "https://twitch.tv/live"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        match.StreamUrl.ShouldBe("https://twitch.tv/live");
    }

    [Fact]
    public async Task Live_match_does_not_move_in_time()
    {
        var match = ScheduleDirectly();
        match.Start(Now);

        var result = await new UpdateMatchCommandHandler(_matches, _unitOfWork)
            .Handle(new UpdateMatchCommand(match.Id, Now.AddDays(3), null), CancellationToken.None);

        result.Error.Code.ShouldBe("match.cannot_reschedule");
        _unitOfWork.SaveChangesCalls.ShouldBe(0);
    }

    [Fact]
    public async Task Untouched_match_is_deleted()
    {
        var match = ScheduleDirectly();

        var result = await Delete(match.Id);

        result.IsSuccess.ShouldBeTrue();
        _matches.Entities.ShouldBeEmpty();
    }

    [Fact]
    public async Task Match_that_has_started_is_kept_as_history()
    {
        var match = ScheduleDirectly();
        match.Start(Now);

        var result = await Delete(match.Id);

        result.Error.ShouldBe(EsportsErrors.MatchHasHistory);
        _matches.Entities.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Match_with_player_stats_is_kept_even_if_it_never_started()
    {
        var match = ScheduleDirectly();
        var player = _world.AddPlayer("Link");
        match.AddPlayerStats(player.Id, _knights.Id, 10, 2, 5, 1500, 1.4m);

        var result = await Delete(match.Id);

        result.Error.ShouldBe(EsportsErrors.MatchHasHistory);
    }

    private Task<ZeldaArena.Domain.Common.Result<Guid>> Schedule(Guid teamA, Guid teamB) =>
        new ScheduleMatchCommandHandler(_matches, _world.Read([_world.Tournament]), new InMemoryQueryExecutor(), _unitOfWork)
            .Handle(
                new ScheduleMatchCommand
                {
                    TournamentId = _world.Tournament.Id,
                    TeamAId = teamA,
                    TeamBId = teamB,
                    ScheduledAt = Now.AddDays(1),
                    BestOf = 3,
                },
                CancellationToken.None);

    private Task<ZeldaArena.Domain.Common.Result> Delete(Guid id) =>
        new DeleteMatchCommandHandler(_matches, _world.Read(_matches.Entities), new InMemoryQueryExecutor(), _unitOfWork)
            .Handle(new DeleteMatchCommand(id), CancellationToken.None);

    private Match ScheduleDirectly()
    {
        var match = Match.Schedule(_world.Tournament.Id, _knights.Id, _gerudo.Id, Now.AddDays(1), 3);
        _matches.AddAsync(match, CancellationToken.None).GetAwaiter().GetResult();

        return match;
    }
}