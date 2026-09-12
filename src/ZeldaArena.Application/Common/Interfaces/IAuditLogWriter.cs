using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}