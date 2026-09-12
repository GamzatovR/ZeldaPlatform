using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.DeleteMatch;

/// <summary>
/// Удаление ошибочно назначенного матча (docs/adr/ADR-0010): матч, который начинался,
/// имеет счёт или статистику, — это история, её не удаляют, а отменяют.
/// </summary>
public sealed record DeleteMatchCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Match);

    public string? AuditEntityId => Id.ToString();
}