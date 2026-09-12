using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.DeletePlayer;

/// <summary>
/// Удаление игрока без истории (docs/adr/ADR-0010): не состоял в командах и не имеет
/// статистики матчей.
/// </summary>
public sealed record DeletePlayerCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Player);

    public string? AuditEntityId => Id.ToString();
}