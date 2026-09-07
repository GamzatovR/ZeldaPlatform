using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Рассылка в реальном времени (docs/SPEC.md §11). Объявлен здесь, реализован в Фазе 10
/// через <c>IHubContext</c> — Application про SignalR не знает и знать не должен.
///
/// Вызывают его обработчики доменных событий, а не хендлеры команд: смена счёта
/// и рассылка зрителям — разные обязанности (§5.5, SRP).
/// </summary>
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