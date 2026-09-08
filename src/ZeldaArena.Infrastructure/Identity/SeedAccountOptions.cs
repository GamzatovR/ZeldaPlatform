namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Учётная запись, создаваемая сидом: администратор или демонстрационный пользователь
/// (docs/SPEC.md §6).
/// </summary>
public sealed class SeedAccountOptions
{
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// В разработке задаётся в appsettings.Development.json — это фикстура, а не секрет.
    /// В любой другой среде приходит переменной окружения SeedAccounts__Admin__Password
    /// (§16, CLAUDE.md «Безопасность»).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}