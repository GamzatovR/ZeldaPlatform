namespace ZeldaArena.Infrastructure.Email;

/// <summary>
/// Настройки SMTP из раздела <c>Email</c> конфигурации. В разработке это MailHog:
/// localhost:1025 без шифрования и без пароля, письма видны на localhost:8025
/// (docs/SPEC.md §7.6).
///
/// Пароль читается из конфигурации, но в репозиторий не попадает: секреты живут
/// в User Secrets и переменных окружения (§16, docs/CONVENTIONS.md «Безопасность»).
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 1025;

    public string From { get; set; } = string.Empty;

    public string FromDisplayName { get; set; } = "ZeldaArena";

    /// <summary>
    /// У MailHog шифрования нет вовсе, поэтому по умолчанию выключено.
    /// На боевом сервере включается вместе с логином и паролем.
    /// </summary>
    public bool UseStartTls { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    /// <summary>Сколько ждать SMTP-сервер, прежде чем считать отправку неудачной.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}