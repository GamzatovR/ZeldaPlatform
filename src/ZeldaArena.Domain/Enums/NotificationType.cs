namespace ZeldaArena.Domain.Enums;

/// <summary>Повод уведомления.</summary>
public enum NotificationType
{
    SubscriptionActivated = 0,
    SubscriptionExpiring = 1,
    SubscriptionExpired = 2,
    PaymentSucceeded = 3,
    PaymentFailed = 4,
    OrderPlaced = 5,
    OrderStatusChanged = 6,
    CommentApproved = 7,
    MatchStarting = 8,
    Announcement = 9,
}