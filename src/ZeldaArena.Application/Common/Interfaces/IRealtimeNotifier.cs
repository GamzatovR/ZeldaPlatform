using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IRealtimeNotifier
{
    /// <summary>Новый счёт зрителям группы <c>match:{id}</c>.</summary>
    Task MatchScoreChangedAsync(
        Guid matchId,
        int scoreA,
        int scoreB,
        CancellationToken cancellationToken = default);

    Task MatchFinishedAsync(
        Guid matchId,
        Guid? winnerTeamId,
        CancellationToken cancellationToken = default);

    /// <summary>Персональное уведомление в группу <c>user:{id}</c>.</summary>
    Task NotifyUserAsync(
        Guid userId,
        NotificationType type,
        string message,
        string? url,
        CancellationToken cancellationToken = default);

    /// <summary>Широковещательное объявление от администратора.</summary>
    Task BroadcastAsync(string message, CancellationToken cancellationToken = default);
}