using Microsoft.AspNetCore.Identity;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Реализация ISignInService поверх.</summary>
public sealed class IdentitySignInService(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IDateTimeProvider dateTimeProvider)
    : ISignInService
{
    public async Task<SignInOutcome> PasswordSignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);

        if (user is null)
        {
            // Проверка пароля не запускается, но и подсказки об этом наружу не уходит:
            // ответ на несуществующий адрес неотличим от ответа на неверный пароль.
            return SignInOutcome.Failed;
        }

        var result = await signInManager
            .PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true)
            .ConfigureAwait(false);

        return Translate(result);
    }

    public async Task<SignInOutcome> TwoFactorSignInAsync(
        string code,
        bool rememberMe,
        bool rememberDevice,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        cancellationToken.ThrowIfCancellationRequested();

        // Пробелы и дефисы в коде — обычное дело при переносе из аутентификатора вручную.
        var normalized = code.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        var result = await signInManager
            .TwoFactorAuthenticatorSignInAsync(normalized, rememberMe, rememberDevice)
            .ConfigureAwait(false);

        return Translate(result);
    }

    public async Task<SignInOutcome> RecoveryCodeSignInAsync(
        string recoveryCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recoveryCode);
        cancellationToken.ThrowIfCancellationRequested();

        var normalized = recoveryCode.Replace(" ", string.Empty, StringComparison.Ordinal);

        var result = await signInManager
            .TwoFactorRecoveryCodeSignInAsync(normalized)
            .ConfigureAwait(false);

        return Translate(result);
    }

    public async Task<UserAccountDto?> GetTwoFactorUserAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await signInManager.GetTwoFactorAuthenticationUserAsync().ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return UserAccountMapper.ToDto(user, [.. roles], dateTimeProvider);
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return signInManager.SignOutAsync();
    }

    public async Task RefreshSignInAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        if (user is not null)
        {
            await signInManager.RefreshSignInAsync(user).ConfigureAwait(false);
        }
    }

    private static SignInOutcome Translate(SignInResult result) => result switch
    {
        { IsLockedOut: true } => SignInOutcome.LockedOut,
        { IsNotAllowed: true } => SignInOutcome.NotAllowed,
        { RequiresTwoFactor: true } => SignInOutcome.RequiresTwoFactor,
        { Succeeded: true } => SignInOutcome.Succeeded,
        _ => SignInOutcome.Failed,
    };
}