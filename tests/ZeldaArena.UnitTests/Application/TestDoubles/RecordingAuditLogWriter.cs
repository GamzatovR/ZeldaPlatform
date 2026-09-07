using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class RecordingAuditLogWriter : IAuditLogWriter
{
    private readonly List<AuditLogEntry> _entries = [];

    /// <summary>Если задано, запись падает — так проверяется, что сбой журнала не роняет команду.</summary>
    public Exception? FailWith { get; init; }

    public IReadOnlyList<AuditLogEntry> Entries => _entries;

    public AuditLogEntry Single => _entries.ShouldHaveSingleItem();

    public Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (FailWith is not null)
        {
            return Task.FromException(FailWith);
        }

        _entries.Add(entry);

        return Task.CompletedTask;
    }
}