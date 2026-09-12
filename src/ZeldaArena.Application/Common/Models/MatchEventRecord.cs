namespace ZeldaArena.Application.Common.Models;

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