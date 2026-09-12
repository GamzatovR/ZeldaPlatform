using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.RemovePlayerFromMyTeam;

public sealed record RemovePlayerFromMyTeamCommand(Guid TeamId, Guid PlayerId) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}