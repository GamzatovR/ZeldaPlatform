using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.RemovePlayerFromMyTeam;

/// <summary>
/// Убрать игрока из состава. Запись состава закрывается датой ухода, а не удаляется:
/// состав историчен (CLAUDE.md), и прошлые матчи обязаны показывать тех, кто в них играл.
/// </summary>
public sealed record RemovePlayerFromMyTeamCommand(Guid TeamId, Guid PlayerId) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}