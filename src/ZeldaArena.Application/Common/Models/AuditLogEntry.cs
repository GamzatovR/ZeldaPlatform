namespace ZeldaArena.Application.Common.Models;

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