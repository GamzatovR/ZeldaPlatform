namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Событие ленты матча (docs/SPEC.md §12, коллекция <c>match_events</c>). Схема гибкая
/// и намеренно не является доменной сущностью: лента живёт в MongoDB и не участвует
/// в инвариантах матча.
/// </summary>
public sealed record MatchEventRecord
{
    public string? Id { get; init; }

    public required Guid MatchId { get; init; }

    /// <summary>Тип события: старт, взятие раунда, завершение и так далее.</summary>
    public required string Type { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public int? Minute { get; init; }

    public Guid? TeamId { get; init; }

    public string? Description { get; init; }
}