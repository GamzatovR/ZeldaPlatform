using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.SignInWithTwoFactor;

public sealed record SignInWithTwoFactorCommand(string Code, bool RememberMe, bool RememberDevice)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}