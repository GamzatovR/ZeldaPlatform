using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.AddFreeAgentToMyTeam;

/// <summary>
/// Взять в состав свободного игрока — без открытой записи состава в какой-либо команде.
/// Переманить игрока из чужой команды нельзя (решение Фазы 6, docs/adr/ADR-0008).
/// </summary>
public sealed record AddFreeAgentToMyTeamCommand(Guid TeamId, Guid PlayerId, PlayerRole Role)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}