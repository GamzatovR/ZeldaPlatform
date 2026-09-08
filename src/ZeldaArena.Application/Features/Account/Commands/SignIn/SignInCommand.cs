using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Commands.SignIn;

/// <summary>
/// Вход по адресу и паролю. Возвращает исход, потому что страница входа обязана
/// развести продолжения: главная, форма второго фактора или сообщение об ошибке
/// (docs/SPEC.md §8.2).
///
/// Пароль в аудит не попадёт — имя поля закрыто SensitiveProperties. Адрес попадёт,
/// и это нужно: без него запись о неудачном входе бесполезна для разбора.
/// </summary>
public sealed record SignInCommand(string Email, string Password, bool RememberMe)
    : ICommand<SignInOutcome>, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    /// <summary>
    /// Идентификатор не указывается сознательно: на момент неудачного входа он либо
    /// неизвестен, либо его подстановка означала бы подтверждение, что адрес заведён.
    /// </summary>
    public string? AuditEntityId => null;
}