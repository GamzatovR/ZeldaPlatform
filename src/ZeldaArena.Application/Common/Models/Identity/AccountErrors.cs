using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Models.Identity;

/// <summary>
/// Ошибки сценариев аккаунта. Собраны в одном месте, потому что ими пользуются
/// и хендлеры Application, и переводчик ошибок Identity в Infrastructure: иначе
/// один и тот же исход назывался бы в двух слоях по-разному.
///
/// Error.Code — ключ ресурса, а не готовая фраза (docs/SPEC.md §9.5): текст
/// подставит IStringLocalizer на слое представления, поле Message остаётся
/// нейтральным текстом для логов.
/// </summary>
public static class AccountErrors
{
    /// <summary>
    /// Единственный ответ на неверную пару логин-пароль, на несуществующий адрес
    /// и на неверный формат — форма не должна подсказывать, какие адреса заведены.
    /// </summary>
    public static readonly Error InvalidCredentials =
        new("account.invalid_credentials", "Неверный адрес электронной почты или пароль.");

    public static readonly Error LockedOut =
        new("account.locked_out", "Вход временно заблокирован из-за неудачных попыток.");

    public static readonly Error EmailNotConfirmed =
        new("account.email_not_confirmed", "Адрес электронной почты не подтверждён.");

    /// <summary>Блокировка администратором — в отличие от временного lockout, бессрочна.</summary>
    public static readonly Error Blocked =
        new("account.blocked", "Учётная запись заблокирована администратором.");

    public static readonly Error EmailAlreadyTaken =
        new("account.email_taken", "Этот адрес электронной почты уже занят.");

    public static readonly Error UserNotFound =
        new("account.user_not_found", "Пользователь не найден.");

    public static readonly Error InvalidToken =
        new("account.invalid_token", "Ссылка недействительна или устарела.");

    public static readonly Error PasswordRejected =
        new("account.password_rejected", "Пароль не удовлетворяет требованиям.");

    public static readonly Error IncorrectPassword =
        new("account.incorrect_password", "Текущий пароль указан неверно.");

    public static readonly Error TwoFactorCodeInvalid =
        new("account.two_factor_code_invalid", "Код из приложения-аутентификатора неверен.");

    public static readonly Error RecoveryCodeInvalid =
        new("account.recovery_code_invalid", "Код восстановления неверен или уже использован.");

    public static readonly Error TwoFactorNotEnabled =
        new("account.two_factor_not_enabled", "Двухфакторная аутентификация не включена.");

    /// <summary>
    /// Сессия второго фактора живёт в отдельной cookie и истекает: пользователь ввёл
    /// пароль, ушёл, вернулся через час — вводить пароль придётся заново.
    /// </summary>
    public static readonly Error TwoFactorSessionExpired =
        new("account.two_factor_session_expired", "Сессия входа истекла, начните заново.");

    /// <summary>Обобщённая неудача Identity, для которой нет своего ключа.</summary>
    public static readonly Error CannotBlockSelf =
        new("account.cannot_block_self", "Нельзя заблокировать самого себя.");

    public static readonly Error CannotDemoteSelf =
        new("account.cannot_demote_self", "Нельзя снять роль администратора с самого себя.");

    public static readonly Error LastAdministrator =
        new("account.last_administrator", "Это последний администратор — снять роль нельзя.");

    public static readonly Error RoleNotAssignable =
        new("account.role_not_assignable", "Такую роль назначить нельзя.");

    public static readonly Error OperationFailed =
        new("account.operation_failed", "Не удалось выполнить операцию.");
}