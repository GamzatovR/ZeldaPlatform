using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.SignInWithRecoveryCode;

public sealed record SignInWithRecoveryCodeCommand(string RecoveryCode)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}