using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamPlayerRole;

/// <summary>Сменить роль игрока в составе своей команды.</summary>
public sealed record ChangeMyTeamPlayerRoleCommand(Guid TeamId, Guid PlayerId, PlayerRole Role)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}