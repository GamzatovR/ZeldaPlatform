using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.SetTeamApproval;

/// <summary>
/// Одобрение команды, созданной подписчиком (docs/SPEC.md §9.4, п. 4). До одобрения
/// команда скрыта со всех публичных страниц (docs/adr/ADR-0008), поэтому это решение
/// о её появлении на сайте — аудируется.
/// </summary>
public sealed record SetTeamApprovalCommand(Guid Id, bool IsApproved) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => Id.ToString();
}