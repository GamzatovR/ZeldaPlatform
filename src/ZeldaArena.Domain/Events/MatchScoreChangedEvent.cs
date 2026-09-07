using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Events;

/// <summary>
/// Счёт матча изменён. Обработчик Фазы 10 рассылает новый счёт зрителям группы
/// match:{id} через IRealtimeNotifier — сама сущность о SignalR не знает
/// (docs/SPEC.md §5.5 SRP, §11).
/// </summary>
public sealed record MatchScoreChangedEvent(
    Guid MatchId,
    Guid TournamentId,
    int ScoreA,
    int ScoreB) : DomainEvent;