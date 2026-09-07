using Microsoft.AspNetCore.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Пользователь платформы: ASP.NET Identity, расширенный полями из docs/SPEC.md §6.
///
/// Живёт в Infrastructure, а не в Domain, потому что Domain не знает об Identity
/// и об ASP.NET Core вообще. Доменные сущности ссылаются на пользователя простым
/// Guid, а внешние ключи на AspNetUsers объявлены в конфигурациях этого слоя.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }

    public string? AvatarPath { get; set; }

    /// <summary>Выбранный язык интерфейса: ru или en (docs/SPEC.md §9.5).</summary>
    public string? PreferredCulture { get; set; }

    public string? CountryCode { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>Блокировка администратором. Отличается от Identity-lockout, который временный.</summary>
    public bool IsBlocked { get; set; }
}