using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Infrastructure.Common;

/// <summary>
/// Системное время. Обёртка над <see cref="TimeProvider"/>, а не прямое обращение
/// к <c>DateTimeOffset.UtcNow</c>: в тестах подменяется и провайдер, и порт.
/// </summary>
public sealed class SystemDateTimeProvider(TimeProvider timeProvider) : IDateTimeProvider
{
    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();

    public DateOnly Today => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}