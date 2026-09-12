using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.AddTournamentTeam;

/// <summary>Команда в составе участников турнира.</summary>
public sealed record AddTournamentTeamCommand(Guid TournamentId, Guid TeamId, int Seed)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Tournament);

    public string? AuditEntityId => TournamentId.ToString();
}