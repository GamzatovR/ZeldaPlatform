using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Commands.EnableTwoFactor;

/// <summary>
/// Включение второго фактора. Возвращает коды восстановления: показать их можно
/// ровно один раз, дальше Identity хранит их так, что прочитать нельзя (§8.2).
///
/// Включение 2FA — событие из обязательного списка аудита §8.2. Код подтверждения
/// в запись не попадёт: поле закрыто SensitiveProperties.
/// </summary>
public sealed record EnableTwoFactorCommand(string VerificationCode)
    : ICommand<RecoveryCodes>, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}