using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.ChangeTeamPlayerRole;

public sealed record ChangeTeamPlayerRoleCommand(Guid TeamId, Guid PlayerId, PlayerRole Role)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}