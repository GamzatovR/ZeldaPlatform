using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Infrastructure.Logging;

/// <summary>
/// Временный приёмник аудита: запись уходит в обычный лог. Настоящее хранилище —
/// коллекция <c>audit_logs</c> в MongoDB — появляется в Фазе 10 вместе с драйвером
/// и страницей /admin/audit (docs/SPEC.md §12, §13).
///
/// Заменяется одной строкой в DependencyInjection: ни конвейер, ни хендлеры,
/// ни Web об этом не узнают. Это и есть DIP в работе (§5.5).
/// </summary>
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