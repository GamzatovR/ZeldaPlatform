using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Events;

/// <summary>Матч завершён: победитель выведен из счёта, серия закрыта.</summary>
public sealed record MatchFinishedEvent(
    Guid MatchId,
    Guid TournamentId,
    Guid WinnerTeamId,
    int ScoreA,
    int ScoreB) : DomainEvent;