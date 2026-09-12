using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournamentTeam;

/// <summary>
/// Посев участника и его итоговое место. Место необязательно: оно проставляется,
/// когда турнир доигран, и до тех пор остаётся пустым.
/// </summary>
public sealed record UpdateTournamentTeamCommand(Guid TournamentId, Guid TeamId, int Seed, int? Placement)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Tournament);

    public string? AuditEntityId => TournamentId.ToString();
}