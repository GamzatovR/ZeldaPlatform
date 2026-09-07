using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

/// <summary>
/// Изменение счёта матча из пульта модератора (docs/SPEC.md §11). Главный сценарий
/// демонстрации SignalR: счёт меняется в одном окне и мгновенно появляется в другом.
///
/// Команда помечена как аудируемая: правка счёта — именно то действие, о котором
/// нужно знать, кто и когда его сделал (§13).
/// </summary>
public sealed record UpdateMatchScoreCommand(Guid MatchId, int ScoreA, int ScoreB)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Domain.Esports.Match);

    public string? AuditEntityId => MatchId.ToString();
}