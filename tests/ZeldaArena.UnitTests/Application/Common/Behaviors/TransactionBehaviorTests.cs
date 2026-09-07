using ZeldaArena.Application.Common.Behaviors;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Common;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Common.Behaviors;

public class TransactionBehaviorTests
{
    [Fact]
    public async Task Successful_command_is_committed()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = new TransactionBehavior<TestCommand, Result>(unitOfWork);

        var result = await behavior.Handle(
            new TestCommand(),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        unitOfWork.Committed.ShouldBeTrue();
        unitOfWork.RolledBack.ShouldBeFalse();
    }

    [Fact]
    public async Task Failed_command_rolls_back_and_rethrows()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var behavior = new TransactionBehavior<TestCommand, Result>(unitOfWork);

        await Should.ThrowAsync<InvalidOperationException>(() => behavior.Handle(
            new TestCommand(),
            _ => throw new InvalidOperationException("хендлер упал"),
            CancellationToken.None));

        unitOfWork.RolledBack.ShouldBeTrue();
        unitOfWork.Committed.ShouldBeFalse();
    }

    private sealed record TestCommand : ICommand;
}