using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.DeleteTournament;

/// <summary>
/// Удаление турнира без истории (docs/adr/ADR-0010): турнир с матчами не удаляется,
/// его можно только отменить.
/// </summary>
public sealed record DeleteTournamentCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Tournament);

    public string? AuditEntityId => Id.ToString();
}