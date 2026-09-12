namespace ZeldaArena.Application.Common.Models;

public sealed record ChatMessageRecord
{
    public string? Id { get; init; }

    public required Guid MatchId { get; init; }

    public required Guid UserId { get; init; }

    public required string UserName { get; init; }

    public required string Text { get; init; }

    public required DateTimeOffset SentAt { get; init; }

    /// <summary>Роль автора на момент отправки — для бейджа в разметке.</summary>
    public string? UserRole { get; init; }

    public bool IsPremium { get; init; }

    public bool IsDeleted { get; init; }
}