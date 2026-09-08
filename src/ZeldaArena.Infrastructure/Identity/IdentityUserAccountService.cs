using Microsoft.AspNetCore.Identity;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Реализация <see cref="IUserAccountService"/> поверх <see cref="UserManager{TUser}"/>.
///
/// Класс сознательно тонкий: он переводит вызовы и результаты, но не принимает решений.
/// Правила «блокированного не пускать», «письмо отправить после регистрации», «после
/// смены пароля разлогинить остальных» живут в хендлерах Application, иначе
/// бизнес-логика расползлась бы по инфраструктуре (CLAUDE.md).
/// </summary>
public sealed class IdentityUserAccountService(
    UserManager<ApplicationUser> userManager,
    IDateTimeProvider dateTimeProvider)
    : IUserAccountService
{
    public async Task<UserAccountDto?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        return user is null ? null : await ToDtoAsync(user).ConfigureAwait(false);
    }

    public async Task<UserAccountDto?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);

        return user is null ? null : await ToDtoAsync(user).ConfigureAwait(false);
    }

    public async Task<Result<Guid>> CreateAsync(
        string email,
        string password,
        string? displayName,
        string? preferredCulture,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            PreferredCulture = preferredCulture,
            CreatedAt = dateTimeProvider.UtcNow,
        };

        var created = await userManager.CreateAsync(user, password).ConfigureAwait(false);

        if (!created.Succeeded)
        {
            return Result.Failure<Guid>(IdentityErrorTranslator.ToError(created));
        }

        // Базовая роль выдаётся сразу: без неё у пользователя не было бы ни одной,
        // а политики §8.1 построены на ролях.
        var assigned = await userManager.AddToRoleAsync(user, RoleNames.User).ConfigureAwait(false);

        return assigned.Succeeded
            ? Result.Success(user.Id)
            : Result.Failure<Guid>(IdentityErrorTranslator.ToError(assigned));
    }

    public Task<Result> AddToRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return WithUserAsync(
            userId,
            async user => IdentityErrorTranslator.ToResult(
                await userManager.AddToRoleAsync(user, role).ConfigureAwait(false)),
            cancellationToken);
    }

    public Task<Result> RemoveFromRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return WithUserAsync(
            userId,
            async user => IdentityErrorTranslator.ToResult(
                await userManager.RemoveFromRoleAsync(user, role).ConfigureAwait(false)),
            cancellationToken);
    }

    public Task<Result<string>> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        WithUserValueAsync(
            userId,
            user => userManager.GenerateEmailConfirmationTokenAsync(user),
            cancellationToken);

    public Task<Result> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return WithUserAsync(
            userId,
            async user => IdentityErrorTranslator.ToResult(
                await userManager.ConfirmEmailAsync(user, token).ConfigureAwait(false)),
            cancellationToken);
    }

    public Task<Result<string>> GeneratePasswordResetTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        WithUserValueAsync(
            userId,
            user => userManager.GeneratePasswordResetTokenAsync(user),
            cancellationToken);

    public Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

        return WithUserAsync(
            userId,
            async user => IdentityErrorTranslator.ToResult(
                await userManager
                    .ResetPasswordAsync(user, token, newPassword)
                    .ConfigureAwait(false)),
            cancellationToken);
    }

    public Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

        return WithUserAsync(
            userId,
            async user => IdentityErrorTranslator.ToResult(
                await userManager
                    .ChangePasswordAsync(user, currentPassword, newPassword)
                    .ConfigureAwait(false)),
            cancellationToken);
    }

    public Task<Result<string>> GenerateEmailChangeTokenAsync(
        Guid userId,
        string newEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);

        return WithUserValueAsync(
            userId,
            user => userManager.GenerateChangeEmailTokenAsync(user, newEmail),
            cancellationToken);
    }

    public Task<Result> ChangeEmailAsync(
        Guid userId,
        string newEmail,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return WithUserAsync(
            userId,
            async user =>
            {
                var changed = await userManager
                    .ChangeEmailAsync(user, newEmail, token)
                    .ConfigureAwait(false);

                if (!changed.Succeeded)
                {
                    return IdentityErrorTranslator.ToResult(changed);
                }

                // Логин совпадает с адресом. Если его не обновить, человек сменит почту
                // и обнаружит, что входить по-прежнему надо под старым адресом.
                return IdentityErrorTranslator.ToResult(
                    await userManager.SetUserNameAsync(user, newEmail).ConfigureAwait(false));
            },
            cancellationToken);
    }

    public Task<Result> UpdateProfileAsync(
        Guid userId,
        string? displayName,
        string? countryCode,
        string? preferredCulture,
        CancellationToken cancellationToken = default) =>
        WithUserAsync(
            userId,
            async user =>
            {
                user.DisplayName = displayName;
                user.CountryCode = countryCode;
                user.PreferredCulture = preferredCulture;

                return IdentityErrorTranslator.ToResult(
                    await userManager.UpdateAsync(user).ConfigureAwait(false));
            },
            cancellationToken);

    public Task<Result> RecordSignInAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        WithUserAsync(
            userId,
            async user =>
            {
                user.LastLoginAt = dateTimeProvider.UtcNow;

                return IdentityErrorTranslator.ToResult(
                    await userManager.UpdateAsync(user).ConfigureAwait(false));
            },
            cancellationToken);

    public Task<Result> InvalidateOtherSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        WithUserAsync(
            userId,
            async user => IdentityErrorTranslator.ToResult(
                await userManager.UpdateSecurityStampAsync(user).ConfigureAwait(false)),
            cancellationToken);

    private async Task<UserAccountDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return UserAccountMapper.ToDto(user, [.. roles], dateTimeProvider);
    }

    /// <summary>
    /// Находит пользователя и выполняет над ним операцию. Отсутствие пользователя —
    /// ожидаемый исход: ссылку из письма могли открыть после удаления учётной записи.
    /// Поэтому Result, а не исключение.
    /// </summary>
    private async Task<Result> WithUserAsync(
        Guid userId,
        Func<ApplicationUser, Task<Result>> operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        return user is null
            ? Result.Failure(AccountErrors.UserNotFound)
            : await operation(user).ConfigureAwait(false);
    }

    private async Task<Result<TValue>> WithUserValueAsync<TValue>(
        Guid userId,
        Func<ApplicationUser, Task<TValue>> operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        return user is null
            ? Result.Failure<TValue>(AccountErrors.UserNotFound)
            : Result.Success(await operation(user).ConfigureAwait(false));
    }
}