using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.SignInWithTwoFactor;

/// <summary>
/// Второй шаг входа: код из аутентификатора.
///
/// Идентификатора пользователя здесь нет намеренно — он берётся из промежуточной
/// cookie, выписанной первым шагом. Приняв его из формы, мы позволили бы войти
/// в чужую учётную запись, зная только код.
/// </summary>
public sealed record SignInWithTwoFactorCommand(string Code, bool RememberMe, bool RememberDevice)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}