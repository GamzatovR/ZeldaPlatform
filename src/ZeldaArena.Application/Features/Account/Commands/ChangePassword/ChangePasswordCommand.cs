using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}