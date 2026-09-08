using Microsoft.AspNetCore.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Срок жизни токена восстановления пароля. Отдельный тип нужен, потому что
/// <see cref="DataProtectionTokenProviderOptions"/> один на все токены Identity,
/// а по docs/SPEC.md §8.2 короткий срок требуется только для сброса пароля:
/// ссылка подтверждения почты, живущая полчаса, раздражала бы без всякой пользы.
/// </summary>
public sealed class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions;