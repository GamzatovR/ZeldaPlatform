using Microsoft.AspNetCore.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Срок жизни токена восстановления пароля.</summary>
public sealed class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions;