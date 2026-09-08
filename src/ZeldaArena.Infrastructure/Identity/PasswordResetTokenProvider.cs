using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Провайдер токенов сброса пароля со своим сроком жизни
/// (<see cref="PasswordResetTokenProviderOptions"/>, 30 минут по docs/SPEC.md §8.2).
/// Отличается от стандартного только источником настроек.
/// </summary>
public sealed class PasswordResetTokenProvider<TUser>(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<PasswordResetTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<TUser>> logger)
    : DataProtectorTokenProvider<TUser>(dataProtectionProvider, options, logger)
    where TUser : class;