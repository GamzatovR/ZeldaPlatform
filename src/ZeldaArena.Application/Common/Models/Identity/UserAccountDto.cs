namespace ZeldaArena.Application.Common.Models.Identity;

public sealed record UserAccountDto(
    Guid Id,
    string Email,
    string UserName,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    bool IsBlocked,
    bool IsLockedOut,
    string? DisplayName,
    string? AvatarPath,
    string? PreferredCulture,
    string? CountryCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<string> Roles);