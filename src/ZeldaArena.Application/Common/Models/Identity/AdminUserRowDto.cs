namespace ZeldaArena.Application.Common.Models.Identity;

/// <summary>Строка таблицы пользователей админки (docs/SPEC.md §9.4, п. 10).</summary>
public sealed record AdminUserRowDto(
    Guid Id,
    string Email,
    string? DisplayName,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    bool IsBlocked,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<string> Roles);