using ZeldaArena.Application.Common.Events;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.UnitTests.Application.Common.Events;

public class DomainEventNotificationFactoryTests
{
    /// <summary>
    /// Диспетчер видит события только как DomainEvent. Обёртка обязана получиться
    /// по фактическому типу, иначе обработчик конкретного события её не поймает.
    /// </summary>
    [Fact]
    public void Notification_is_built_for_the_concrete_event_type()
    {
        var domainEvent = new MatchScoreChangedEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            2,
            1);

        var notification = DomainEventNotificationFactory.Create(domainEvent);

        var typed = notification.ShouldBeOfType<DomainEventNotification<MatchScoreChangedEvent>>();
        typed.DomainEvent.ShouldBeSameAs(domainEvent);
    }

    [Fact]
    public void Different_events_produce_different_notification_types()
    {
        var scoreChanged = DomainEventNotificationFactory.Create(
            new MatchScoreChangedEvent(Guid.CreateVersion7(), Guid.CreateVersion7(), 1, 0));

        var finished = DomainEventNotificationFactory.Create(
            new MatchFinishedEvent(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                2,
                1));

        scoreChanged.GetType().ShouldNotBe(finished.GetType());
    }

    [Fact]
    public void Null_event_is_a_programming_error() =>
        Should.Throw<ArgumentNullException>(() => DomainEventNotificationFactory.Create(null!));
}