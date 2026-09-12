using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.DeleteTeam;

/// <summary>
/// Удаление команды без истории (docs/adr/ADR-0010): ни матчей, ни участия в турнирах,
/// ни записей состава. Всё остальное снимается с одобрения, а не стирается.
/// </summary>
public sealed record DeleteTeamCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => Id.ToString();
}