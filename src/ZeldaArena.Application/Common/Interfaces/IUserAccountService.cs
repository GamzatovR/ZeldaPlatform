using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IUserAccountService
{
    Task<UserAccountDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserAccountDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Заводит пользователя и выдаёт ему роль User. Возвращает идентификатор.</summary>
    Task<Result<Guid>> CreateAsync(
        string email,
        string password,
        string? displayName,
        string? preferredCulture,
        CancellationToken cancellationToken = default);

    Task<Result> AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);

    Task<Result> RemoveFromRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);

    Task<Result<string>> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);

    Task<Result<string>> GeneratePasswordResetTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    /// <summary>Меняет пароль с проверкой текущего — без него смена запрещена.</summary>
    Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<Result<string>> GenerateEmailChangeTokenAsync(
        Guid userId,
        string newEmail,
        CancellationToken cancellationToken = default);

    Task<Result> ChangeEmailAsync(
        Guid userId,
        string newEmail,
        string token,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateProfileAsync(
        Guid userId,
        string? displayName,
        string? countryCode,
        string? preferredCulture,
        CancellationToken cancellationToken = default);

    Task<Result> RecordSignInAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result> InvalidateOtherSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}