namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Числовые требования к безопасности аккаунта.</summary>
internal static class IdentityPolicy
{
    public const int MaxFailedAccessAttempts = 5;

    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    /// <summary>Как часто cookie сверяется со стампом безопасности в базе.</summary>
    public static readonly TimeSpan SecurityStampValidationInterval = TimeSpan.FromMinutes(5);

    /// <summary>Токен восстановления пароля живёт 30 минут.</summary>
    public static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromMinutes(30);

    /// <summary>«Запомнить устройство» для второго фактора.</summary>
    public static readonly TimeSpan RememberDeviceDuration = TimeSpan.FromDays(30);

    public static readonly TimeSpan SignInCookieLifetime = TimeSpan.FromDays(14);

    /// <summary>Сколько кодов восстановления выдаётся за раз.</summary>
    public const int RecoveryCodeCount = 10;

    /// <summary>Имя провайдера токенов восстановления пароля с укороченным сроком.</summary>
    public const string PasswordResetTokenProviderName = "ZeldaArenaPasswordReset";
}