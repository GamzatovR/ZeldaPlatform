using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.RemoveTeamPlayer;

/// <summary>
/// Игрок покидает состав. Запись не удаляется, а закрывается датой ухода: состав
/// историчен (docs/SPEC.md §6, <c>RosterEntries</c> с <c>JoinedAt</c>/<c>LeftAt</c>).
/// </summary>
public sealed record RemoveTeamPlayerCommand(Guid TeamId, Guid PlayerId) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}