namespace ZeldaArena.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}