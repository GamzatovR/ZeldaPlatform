namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Запись аудита действий (docs/SPEC.md §13). Собирается <c>AuditBehavior</c> вокруг
/// команды и уходит в порт <c>IAuditLogWriter</c>; в Фазе 10 приёмником становится
/// коллекция <c>audit_logs</c> в MongoDB (§12).
///
/// Неуспешный исход пишется наравне с успешным: попытка сделать то, на что нет прав,
/// интереснее для разбора, чем удавшееся действие.
///
/// В <see cref="PayloadJson"/> не должно быть секретов — за это отвечает
/// <c>SensitiveProperties</c>: пароли, токены, номер карты, CVV и код подтверждения
/// в аудит не попадают (§13, §7.6).
/// </summary>
public sealed record AuditLogEntry
{
    /// <summary>Имя команды, например <c>UpdateMatchScoreCommand</c>.</summary>
    public required string Action { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required bool Succeeded { get; init; }

    public long DurationMs { get; init; }

    public Guid? UserId { get; init; }

    public string? UserName { get; init; }

    public string? EntityType { get; init; }

    public string? EntityId { get; init; }

    public string? PayloadJson { get; init; }

    public string? FailureReason { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public string? CorrelationId { get; init; }
}