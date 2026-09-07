using NSubstitute;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.Events;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Matches;

public class UpdateMatchScoreCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 4, 17, 0, 0, TimeSpan.Zero);
    private static readonly Guid TournamentId = Guid.CreateVersion7();
    private static readonly Guid TeamAId = Guid.CreateVersion7();
    private static readonly Guid TeamBId = Guid.CreateVersion7();

    [Fact]
    public async Task Score_of_a_live_match_is_updated_and_saved()
    {
        var match = LiveBo3();
        var unitOfWork = new RecordingUnitOfWork();

        var result = await Handler(match, unitOfWork)
            .Handle(new UpdateMatchScoreCommand(match.Id, 2, 1), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        match.ScoreA.ShouldBe(2);
        match.ScoreB.ShouldBe(1);
        unitOfWork.SaveChangesCalls.ShouldBe(1);
    }

    /// <summary>
    /// Рассылку зрителям делает обработчик события, а не хендлер: его дело — только
    /// поднять событие через сущность (docs/SPEC.md §5.5, SRP).
    /// </summary>
    [Fact]
    public async Task Successful_update_raises_the_domain_event()
    {
        var match = LiveBo3();

        await Handler(match, new RecordingUnitOfWork())
            .Handle(new UpdateMatchScoreCommand(match.Id, 1, 0), CancellationToken.None);

        var raised = match.DomainEvents.OfType<MatchScoreChangedEvent>().ShouldHaveSingleItem();
        raised.MatchId.ShouldBe(match.Id);
        raised.ScoreA.ShouldBe(1);
        raised.ScoreB.ShouldBe(0);
    }

    [Fact]
    public async Task Missing_match_is_an_expected_failure_not_an_exception()
    {
        var repository = Substitute.For<IRepository<Match>>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Match?)null);

        var unitOfWork = new RecordingUnitOfWork();
        var handler = new UpdateMatchScoreCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new UpdateMatchScoreCommand(Guid.CreateVersion7(), 1, 0),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UpdateMatchScoreCommandHandler.MatchNotFound);
        unitOfWork.SaveChangesCalls.ShouldBe(0);
    }

    /// <summary>
    /// Правило формата серии остаётся за сущностью. Хендлер его не повторяет,
    /// поэтому исключение домена доходит наружу и в Фазе 11 станет ответом 409.
    /// </summary>
    [Fact]
    public async Task Score_beyond_the_series_format_is_rejected_by_the_entity()
    {
        var match = LiveBo3();
        var unitOfWork = new RecordingUnitOfWork();

        var exception = await Should.ThrowAsync<InvariantViolationException>(() =>
            Handler(match, unitOfWork).Handle(
                new UpdateMatchScoreCommand(match.Id, 3, 0),
                CancellationToken.None));

        exception.Code.ShouldBe("match.score_exceeds_wins_required");
        unitOfWork.SaveChangesCalls.ShouldBe(0);
    }

    [Fact]
    public async Task Finished_match_is_not_editable()
    {
        var match = LiveBo3();
        match.UpdateScore(2, 0);
        match.Finish(Now.AddHours(1));

        var exception = await Should.ThrowAsync<InvariantViolationException>(() =>
            Handler(match, new RecordingUnitOfWork()).Handle(
                new UpdateMatchScoreCommand(match.Id, 2, 1),
                CancellationToken.None));

        exception.Code.ShouldBe("match.finished_is_read_only");
    }

    private static UpdateMatchScoreCommandHandler Handler(Match match, IUnitOfWork unitOfWork)
    {
        var repository = Substitute.For<IRepository<Match>>();
        repository.GetByIdAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);

        return new UpdateMatchScoreCommandHandler(repository, unitOfWork);
    }

    private static Match LiveBo3()
    {
        var match = Match.Schedule(TournamentId, TeamAId, TeamBId, Now, 3);
        match.Start(Now);
        match.ClearDomainEvents();

        return match;
    }
}