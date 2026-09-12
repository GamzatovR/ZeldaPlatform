using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Account;

namespace ZeldaArena.Application.Features.Admin.Users.Commands.SetUserBlocked;

/// <summary>
/// Блокировка учётной записи (docs/SPEC.md §9.4, п. 10). Аудируется: это действие
/// над чужим доступом (§8.2).
/// </summary>
public sealed record SetUserBlockedCommand(Guid UserId, bool IsBlocked) : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => UserId.ToString();
}