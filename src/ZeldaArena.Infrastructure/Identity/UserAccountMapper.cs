using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>ApplicationUser → UserAccountDto.</summary>
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

    private static bool IsLockedOut(ApplicationUser user, IDateTimeProvider dateTimeProvider) =>
        user.LockoutEnabled
        && user.LockoutEnd.HasValue
        && user.LockoutEnd.Value > dateTimeProvider.UtcNow;
}