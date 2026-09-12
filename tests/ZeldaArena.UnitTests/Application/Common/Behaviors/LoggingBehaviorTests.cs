using ZeldaArena.Application.Common.Behaviors;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Common;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Common.Behaviors;

public class LoggingBehaviorTests
{
    private const string CardNumber = "4111111111111111";

    [Fact]
    public async Task Start_and_finish_are_logged_with_request_name()
    {
        var logger = new RecordingLogger<LoggingBehavior<TestCommand, Result>>();

        await Behavior(logger).Handle(
            new TestCommand(CardNumber),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        logger.Messages.Count.ShouldBe(2);
        logger.Messages.ShouldAllBe(message => message.Contains(nameof(TestCommand)));
    }

    [Fact]
    public async Task Failure_is_logged_and_rethrown()
    {
        var logger = new RecordingLogger<LoggingBehavior<TestCommand, Result>>();

        await Should.ThrowAsync<InvalidOperationException>(() => Behavior(logger).Handle(
            new TestCommand(CardNumber),
            _ => throw new InvalidOperationException("сценарий упал"),
            CancellationToken.None));

        logger.Messages.ShouldContain(message => message.Contains("упал"));
    }

    [Fact]
    public async Task Request_content_never_reaches_the_log()
    {
        var logger = new RecordingLogger<LoggingBehavior<TestCommand, Result>>();

        await Behavior(logger).Handle(
            new TestCommand(CardNumber),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        logger.Messages.ShouldAllBe(message => !message.Contains(CardNumber));
    }

    private static LoggingBehavior<TestCommand, Result> Behavior(
        RecordingLogger<LoggingBehavior<TestCommand, Result>> logger) =>
        new(logger, new StubCurrentUserService { UserId = Guid.CreateVersion7() });

    private sealed record TestCommand(string CardNumber) : ICommand;
}