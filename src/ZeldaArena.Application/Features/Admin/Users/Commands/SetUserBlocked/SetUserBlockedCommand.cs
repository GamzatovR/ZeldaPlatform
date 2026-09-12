using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Account;

namespace ZeldaArena.Application.Features.Admin.Users.Commands.SetUserBlocked;

public sealed record SetUserBlockedCommand(Guid UserId, bool IsBlocked) : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => UserId.ToString();
}