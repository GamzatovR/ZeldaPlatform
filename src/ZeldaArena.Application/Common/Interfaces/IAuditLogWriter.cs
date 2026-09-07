using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Приёмник записей аудита (docs/SPEC.md §13). В Фазе 2 реализация пишет через
/// <c>ILogger</c>, в Фазе 10 её место занимает <c>MongoAuditLogWriter</c> с коллекцией
/// <c>audit_logs</c> — меняется одна строка регистрации, ни Application, ни Web
/// об этом не узнают (§5.5, DIP).
/// </summary>
public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}