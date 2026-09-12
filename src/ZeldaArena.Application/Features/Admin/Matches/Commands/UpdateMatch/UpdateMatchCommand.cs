using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.UpdateMatch;

public sealed record UpdateMatchCommand(Guid Id, DateTimeOffset ScheduledAt, string? StreamUrl)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Match);

    public string? AuditEntityId => Id.ToString();
}