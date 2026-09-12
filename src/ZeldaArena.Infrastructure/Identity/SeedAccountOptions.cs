namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Учётная запись, создаваемая сидом.</summary>
public sealed class SeedAccountOptions
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}