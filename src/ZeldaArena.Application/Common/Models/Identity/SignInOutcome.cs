namespace ZeldaArena.Application.Common.Models.Identity;

public enum SignInOutcome
{
    /// <summary>Пара логин-пароль не подошла.</summary>
    Failed = 0,

    Succeeded = 1,

    /// <summary>Пароль верен, включена 2FA — нужен код из аутентификатора.</summary>
    RequiresTwoFactor = 2,

    /// <summary>Сработал lockout: 5 неудачных попыток, 15 минут ожидания.</summary>
    LockedOut = 3,

    /// <summary>Вход запрещён — как правило, не подтверждён email.</summary>
    NotAllowed = 4,
}