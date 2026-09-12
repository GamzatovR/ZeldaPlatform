using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.RemoveTournamentTeam;

public sealed record RemoveTournamentTeamCommand(Guid TournamentId, Guid TeamId)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Tournament);

    public string? AuditEntityId => TournamentId.ToString();
}