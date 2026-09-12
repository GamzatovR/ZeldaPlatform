using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Identity;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Двухфакторная аутентификация по TOTP.</summary>
public sealed class IdentityTwoFactorService(UserManager<ApplicationUser> userManager)
    : ITwoFactorService
{
    /// <summary>Имя издателя в аутентификаторе: под ним запись видна в списке.</summary>
    private const string Issuer = "ZeldaArena";

    /// <summary>Ключ читается человеком с экрана, поэтому режется на группы по четыре.</summary>
    private const int KeyGroupSize = 4;

    public async Task<Result<TwoFactorSetup>> GetSetupAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure<TwoFactorSetup>(AccountErrors.UserNotFound);
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(false);

        if (string.IsNullOrEmpty(key))
        {
            // Ключа ещё нет — заводим.
            await userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(false);
            key = await userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(false);
        }

        if (string.IsNullOrEmpty(key))
        {
            return Result.Failure<TwoFactorSetup>(AccountErrors.OperationFailed);
        }

        var email = user.Email ?? user.UserName ?? user.Id.ToString();

        return Result.Success(new TwoFactorSetup(FormatKey(key), BuildAuthenticatorUri(email, key)));
    }

    public async Task<Result> EnableAsync(
        Guid userId,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(verificationCode);
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var normalized = verificationCode
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        var verified = await userManager
            .VerifyTwoFactorTokenAsync(
                user,
                userManager.Options.Tokens.AuthenticatorTokenProvider,
                normalized)
            .ConfigureAwait(false);

        if (!verified)
        {
            // Включать 2FA до успешной проверки нельзя: неверно настроенный
            // аутентификатор запер бы человека снаружи собственной учётной записи.
            return Result.Failure(AccountErrors.TwoFactorCodeInvalid);
        }

        return IdentityErrorTranslator.ToResult(
            await userManager.SetTwoFactorEnabledAsync(user, enabled: true).ConfigureAwait(false));
    }

    public async Task<Result> DisableAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var disabled = await userManager
            .SetTwoFactorEnabledAsync(user, enabled: false)
            .ConfigureAwait(false);

        if (!disabled.Succeeded)
        {
            return IdentityErrorTranslator.ToResult(disabled);
        }

        // Старый секрет вместе с 2FA не остаётся: если человек включит её снова,
        // он должен получить новый QR-код, а не тот, что мог утечь.
        await userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(false);

        return Result.Success();
    }

    public async Task<Result<RecoveryCodes>> GenerateRecoveryCodesAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure<RecoveryCodes>(AccountErrors.UserNotFound);
        }

        if (!await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false))
        {
            return Result.Failure<RecoveryCodes>(AccountErrors.TwoFactorNotEnabled);
        }

        var codes = await userManager
            .GenerateNewTwoFactorRecoveryCodesAsync(user, count)
            .ConfigureAwait(false);

        return Result.Success(new RecoveryCodes([.. codes ?? []]));
    }

    public async Task<Result<int>> CountRemainingRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        return user is null
            ? Result.Failure<int>(AccountErrors.UserNotFound)
            : Result.Success(
                await userManager.CountRecoveryCodesAsync(user).ConfigureAwait(false));
    }

    private static string FormatKey(string key)
    {
        var builder = new StringBuilder();

        for (var position = 0; position < key.Length; position += KeyGroupSize)
        {
            if (position > 0)
            {
                builder.Append(' ');
            }

            builder.Append(key.AsSpan(position, Math.Min(KeyGroupSize, key.Length - position)));
        }

        return builder.ToString().ToLowerInvariant();
    }

    private static string BuildAuthenticatorUri(string email, string key) =>
        string.Format(
            CultureInfo.InvariantCulture,
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            UrlEncoder.Default.Encode(Issuer),
            UrlEncoder.Default.Encode(email),
            key);
}