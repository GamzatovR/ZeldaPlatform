using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournamentTeam;

public sealed record UpdateTournamentTeamCommand(Guid TournamentId, Guid TeamId, int Seed, int? Placement)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Tournament);

    public string? AuditEntityId => TournamentId.ToString();
}