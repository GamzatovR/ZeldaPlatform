using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.AddTeamPlayer;

/// <summary>Игрок в состав команды из админки.</summary>
public sealed record AddTeamPlayerCommand(Guid TeamId, Guid PlayerId, PlayerRole Role)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}