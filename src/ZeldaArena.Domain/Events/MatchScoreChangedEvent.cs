using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Events;

/// <summary>Счёт матча изменён.</summary>
public sealed record MatchScoreChangedEvent(
    Guid MatchId,
    Guid TournamentId,
    int ScoreA,
    int ScoreB) : DomainEvent;