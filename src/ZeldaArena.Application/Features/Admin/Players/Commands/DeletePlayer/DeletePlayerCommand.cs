using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.DeletePlayer;

public sealed record DeletePlayerCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Player);

    public string? AuditEntityId => Id.ToString();
}