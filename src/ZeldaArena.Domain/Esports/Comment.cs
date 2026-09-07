using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Domain.Esports;

/// <summary>
/// Комментарий к новости или матчу. Удаление мягкое: модератор скрывает текст,
/// но ветка обсуждения и авторство остаются для аудита (docs/SPEC.md §9.4).
/// </summary>
public class Comment : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public const int MaxLength = 2000;

    private Comment()
    {
    }

    public Guid UserId { get; private set; }

    public CommentTargetType TargetType { get; private set; }

    public Guid TargetId { get; private set; }

    /// <summary>Простой текст. Разметка не поддерживается, Razor экранирует вывод сам.</summary>
    public string Text { get; private set; } = null!;

    public bool IsApproved { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Comment Create(
        Guid userId,
        CommentTargetType targetType,
        Guid targetId,
        string text,
        bool isApproved = false)
    {
        InvariantViolationException.ThrowIf(
            userId == Guid.Empty || targetId == Guid.Empty,
            "comment.empty_reference",
            "Комментарию нужны и автор, и цель.");

        return new Comment
        {
            UserId = userId,
            TargetType = targetType,
            TargetId = targetId,
            Text = RequireText(text),
            IsApproved = isApproved,
        };
    }

    public void Edit(string text)
    {
        InvariantViolationException.ThrowIf(
            IsDeleted,
            "comment.deleted_is_read_only",
            "Удалённый комментарий редактировать нельзя.");

        Text = RequireText(text);
    }

    public void Approve() => IsApproved = true;

    public void Reject() => IsApproved = false;

    public void Delete(DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        DeletedAt = deletedAt;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }

    private static string RequireText(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var normalized = text.Trim();

        InvariantViolationException.ThrowIf(
            normalized.Length > MaxLength,
            "comment.too_long",
            $"Комментарий длиннее {MaxLength} символов.");

        return normalized;
    }
}