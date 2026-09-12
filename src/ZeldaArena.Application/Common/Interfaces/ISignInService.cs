using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Common.Interfaces;

public interface ISignInService
{
    Task<SignInOutcome> PasswordSignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    Task<SignInOutcome> TwoFactorSignInAsync(
        string code,
        bool rememberMe,
        bool rememberDevice,
        CancellationToken cancellationToken = default);

    Task<SignInOutcome> RecoveryCodeSignInAsync(
        string recoveryCode,
        CancellationToken cancellationToken = default);

    Task<UserAccountDto?> GetTwoFactorUserAsync(CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    Task RefreshSignInAsync(Guid userId, CancellationToken cancellationToken = default);
}