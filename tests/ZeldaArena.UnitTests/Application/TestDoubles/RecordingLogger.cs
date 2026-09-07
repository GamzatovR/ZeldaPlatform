using Microsoft.Extensions.Logging;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Логгер, запоминающий записи. Нужен, чтобы проверить, что в лог не утекают
/// содержимое команды и секреты (docs/SPEC.md §13).
/// </summary>
internal sealed class RecordingLogger<TCategory> : ILogger<TCategory>
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages => _messages;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        _messages.Add(formatter(state, exception));
    }
}