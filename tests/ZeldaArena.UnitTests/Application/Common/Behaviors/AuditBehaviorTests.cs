using ZeldaArena.Application.Common.Behaviors;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Common;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Common.Behaviors;

public class AuditBehaviorTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 20, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid MatchId = Guid.CreateVersion7();

    [Fact]
    public async Task Successful_command_is_recorded_with_actor_and_entity()
    {
        var writer = new RecordingAuditLogWriter();

        var result = await Behavior<TestCommand>(writer).Handle(
            Command(),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        var entry = writer.Single;
        entry.Action.ShouldBe(nameof(TestCommand));
        entry.Succeeded.ShouldBeTrue();
        entry.FailureReason.ShouldBeNull();
        entry.UserId.ShouldBe(UserId);
        entry.UserName.ShouldBe("moderator");
        entry.EntityType.ShouldBe("Match");
        entry.EntityId.ShouldBe(MatchId.ToString());
        entry.IpAddress.ShouldBe("203.0.113.7");
        entry.CorrelationId.ShouldBe("corr-1");
        entry.OccurredAt.ShouldBe(Now);
    }

    [Fact]
    public async Task Business_failure_is_recorded_with_error_code()
    {
        var writer = new RecordingAuditLogWriter();

        await Behavior<TestCommand>(writer).Handle(
            Command(),
            _ => Task.FromResult(Result.Failure(new Error("match.not_found", "Матч не найден."))),
            CancellationToken.None);

        writer.Single.Succeeded.ShouldBeFalse();
        writer.Single.FailureReason.ShouldBe("match.not_found");
    }

    [Fact]
    public async Task Thrown_exception_is_recorded_and_rethrown()
    {
        var writer = new RecordingAuditLogWriter();

        await Should.ThrowAsync<InvalidOperationException>(
            () => Behavior<TestCommand>(writer).Handle(
                Command(),
                _ => throw new InvalidOperationException("счёт вне формата серии"),
                CancellationToken.None));

        writer.Single.Succeeded.ShouldBeFalse();
        writer.Single.FailureReason.ShouldBe("счёт вне формата серии");
    }

    [Fact]
    public async Task Sensitive_fields_are_masked_in_the_payload()
    {
        var writer = new RecordingAuditLogWriter();

        await Behavior<PaymentishCommand>(writer).Handle(
            new PaymentishCommand("4111111111111111", "987", "654321", "s3cret", "Заказ 42"),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        var payload = writer.Single.PayloadJson.ShouldNotBeNull();

        payload.ShouldNotContain("4111111111111111");
        payload.ShouldNotContain("987");
        payload.ShouldNotContain("654321");
        payload.ShouldNotContain("s3cret");
        payload.ShouldContain(SensitiveProperties.Mask);

        // Маскируется секрет, а не вся команда: обычные поля остаются читаемыми.
        payload.ShouldContain("Заказ 42");
    }

    [Fact]
    public async Task Ordinary_fields_stay_readable_in_the_payload()
    {
        var writer = new RecordingAuditLogWriter();

        await Behavior<TestCommand>(writer).Handle(
            Command(),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        writer.Single.PayloadJson.ShouldNotBeNull().ShouldContain(MatchId.ToString());
    }

    /// <summary>Недоступный журнал не должен превращаться в ошибку у пользователя.</summary>
    [Fact]
    public async Task Broken_writer_does_not_break_the_command()
    {
        var writer = new RecordingAuditLogWriter
        {
            FailWith = new TimeoutException("журнал недоступен"),
        };

        var result = await Behavior<TestCommand>(writer).Handle(
            Command(),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        writer.Entries.ShouldBeEmpty();
    }

    private static TestCommand Command() => new(MatchId, 2, 1);

    private static AuditBehavior<TRequest, Result> Behavior<TRequest>(
        RecordingAuditLogWriter writer)
        where TRequest : IAuditableRequest =>
        new(
            writer,
            new StubCurrentUserService
            {
                UserId = UserId,
                UserName = "moderator",
                IpAddress = "203.0.113.7",
                UserAgent = "xunit",
                CorrelationId = "corr-1",
            },
            new FixedDateTimeProvider(Now),
            new RecordingLogger<AuditBehavior<TRequest, Result>>());

    private sealed record TestCommand(Guid MatchId, int ScoreA, int ScoreB)
        : ICommand, IAuditableRequest
    {
        public string AuditEntityType => "Match";

        public string? AuditEntityId => MatchId.ToString();
    }

    private sealed record PaymentishCommand(
        string CardNumber,
        string Cvv,
        string ConfirmationCode,
        string Password,
        string Comment) : ICommand, IAuditableRequest
    {
        public string AuditEntityType => "Payment";

        public string? AuditEntityId => null;
    }
}