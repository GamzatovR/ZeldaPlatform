namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Сообщение чата матча (docs/SPEC.md §12, коллекция <c>chat_messages</c>).
/// Имя и признак подписки хранятся снапшотом: история чата должна читаться и после
/// того, как пользователь сменил ник или подписка истекла (§11).
/// </summary>
public sealed record ChatMessageRecord
{
    public string? Id { get; init; }

    public required Guid MatchId { get; init; }

    public required Guid UserId { get; init; }

    public required string UserName { get; init; }

    public required string Text { get; init; }

    public required DateTimeOffset SentAt { get; init; }

    /// <summary>Роль автора на момент отправки — для бейджа в разметке (§11).</summary>
    public string? UserRole { get; init; }

    public bool IsPremium { get; init; }

    public bool IsDeleted { get; init; }
}