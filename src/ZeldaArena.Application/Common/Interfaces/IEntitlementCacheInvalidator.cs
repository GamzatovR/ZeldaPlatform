namespace ZeldaArena.Application.Common.Interfaces;

public interface IEntitlementCacheInvalidator
{
    /// <summary>Права одного пользователя изменились: оплата, истечение, отзыв подписки.</summary>
    Task InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}