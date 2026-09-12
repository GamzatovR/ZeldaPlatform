using Microsoft.AspNetCore.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Пользователь платформы.</summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }

    public string? AvatarPath { get; set; }

    /// <summary>Выбранный язык интерфейса: ru или en.</summary>
    public string? PreferredCulture { get; set; }

    public string? CountryCode { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>Блокировка администратором. Отличается от Identity-lockout, который временный.</summary>
    public bool IsBlocked { get; set; }
}