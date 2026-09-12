using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Account;

namespace ZeldaArena.Application.Features.Admin.Users.Commands.SetUserRoles;

/// <summary>Набор ролей пользователя.</summary>
public sealed record SetUserRolesCommand(Guid UserId, IReadOnlyList<string> Roles)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => UserId.ToString();
}