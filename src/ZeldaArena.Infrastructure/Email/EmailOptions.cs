namespace ZeldaArena.Infrastructure.Email;

/// <summary>Настройки SMTP из раздела Email конфигурации.</summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 1025;

    public string From { get; set; } = string.Empty;

    public string FromDisplayName { get; set; } = "ZeldaArena";

    public bool UseStartTls { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    /// <summary>Сколько ждать SMTP-сервер, прежде чем считать отправку неудачной.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}