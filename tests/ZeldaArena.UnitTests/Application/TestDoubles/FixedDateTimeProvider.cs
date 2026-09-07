using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class FixedDateTimeProvider(DateTimeOffset moment) : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; } = moment;

    public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
}