using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Commands.SignIn;

public sealed record SignInCommand(string Email, string Password, bool RememberMe)
    : ICommand<SignInOutcome>, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}