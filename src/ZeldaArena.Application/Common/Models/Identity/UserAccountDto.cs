namespace ZeldaArena.Application.Common.Models.Identity;

/// <summary>
/// Учётная запись в терминах Application. Нужен, чтобы порты Identity не возвращали
/// наружу ApplicationUser: он живёт в Infrastructure, и его появление в сигнатуре
/// порта затащило бы Microsoft.AspNetCore.Identity в Application, а следом уронило
/// правило 2 docs/SPEC.md §5.2.
///
/// Хеша пароля, стампа безопасности и ключа аутентификатора здесь нет и быть не должно:
/// слою сценариев они не нужны ни для одного решения.
/// </summary>
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