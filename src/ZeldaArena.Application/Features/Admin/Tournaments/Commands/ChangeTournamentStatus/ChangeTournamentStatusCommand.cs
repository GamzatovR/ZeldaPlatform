using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.ChangeTournamentStatus;

public sealed record ChangeTournamentStatusCommand(Guid Id, TournamentTransition Transition)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Tournament);

    public string? AuditEntityId => Id.ToString();
}