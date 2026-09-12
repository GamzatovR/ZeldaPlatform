using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.DeleteMatch;

public sealed record DeleteMatchCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Match);

    public string? AuditEntityId => Id.ToString();
}