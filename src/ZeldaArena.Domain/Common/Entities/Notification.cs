using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Domain.Common.Entities;

/// <summary>Персональное уведомление пользователя.</summary>
public class Notification : BaseEntity
{
    private Notification()
    {
    }

    public Guid UserId { get; private set; }

    public NotificationType Type { get; private set; }

    /// <summary>Данные подстановки: номер заказа, название команды, дата окончания подписки.</summary>
    public string? PayloadJson { get; private set; }

    /// <summary>Ссылка, на которую ведёт уведомление из колокольчика.</summary>
    public string? Url { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        DateTimeOffset createdAt,
        string? payloadJson = null,
        string? url = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);

        return new Notification
        {
            UserId = userId,
            Type = type,
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? null : payloadJson,
            Url = string.IsNullOrWhiteSpace(url) ? null : url.Trim(),
            CreatedAt = createdAt,
        };
    }

    public void MarkRead(DateTimeOffset readAt)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = readAt;
    }
}