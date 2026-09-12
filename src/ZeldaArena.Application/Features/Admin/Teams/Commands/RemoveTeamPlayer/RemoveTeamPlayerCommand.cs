using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.RemoveTeamPlayer;

public sealed record RemoveTeamPlayerCommand(Guid TeamId, Guid PlayerId) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}