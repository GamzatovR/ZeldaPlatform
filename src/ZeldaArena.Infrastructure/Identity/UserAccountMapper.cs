using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// ApplicationUser → UserAccountDto. Граница, за которую тип Identity не проходит.
///
/// Роли приходят отдельным параметром, а не читаются здесь: их выборка — обращение
/// к базе, и решать, нужны они вызывающему или нет, должен вызывающий.
/// </summary>
internal static class UserAccountMapper
{
    public static UserAccountDto ToDto(
        ApplicationUser user,
        IReadOnlyList<string> roles,
        IDateTimeProvider dateTimeProvider) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.EmailConfirmed,
            user.TwoFactorEnabled,
            user.IsBlocked,
            IsLockedOut(user, dateTimeProvider),
            user.DisplayName,
            user.AvatarPath,
            user.PreferredCulture,
            user.CountryCode,
            user.CreatedAt,
            user.LastLoginAt,
            roles);

    /// <summary>
    /// Временный lockout за неудачные попытки (docs/SPEC.md §8.2). С блокировкой
    /// администратором (IsBlocked) не смешивается: та бессрочна и снимается вручную.
    /// </summary>
    private static bool IsLockedOut(ApplicationUser user, IDateTimeProvider dateTimeProvider) =>
        user.LockoutEnabled
        && user.LockoutEnd.HasValue
        && user.LockoutEnd.Value > dateTimeProvider.UtcNow;
}