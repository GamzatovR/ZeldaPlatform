using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Commands.EnableTwoFactor;

public sealed record EnableTwoFactorCommand(string VerificationCode)
    : ICommand<RecoveryCodes>, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}