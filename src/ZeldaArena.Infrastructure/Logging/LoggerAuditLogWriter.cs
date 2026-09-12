using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Infrastructure.Logging;

public sealed class LoggerAuditLogWriter(ILogger<LoggerAuditLogWriter> logger) : IAuditLogWriter
{
    public Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        logger.LogInformation(
            "Аудит: {Action} над {EntityType}/{EntityId}; пользователь {UserId} ({UserName}); "
            + "успех {Succeeded}; причина {FailureReason}; {DurationMs} мс; адрес {IpAddress}; "
            + "correlation {CorrelationId}; данные {PayloadJson}",
            entry.Action,
            entry.EntityType,
            entry.EntityId,
            entry.UserId,
            entry.UserName,
            entry.Succeeded,
            entry.FailureReason,
            entry.DurationMs,
            entry.IpAddress,
            entry.CorrelationId,
            entry.PayloadJson);

        return Task.CompletedTask;
    }
}