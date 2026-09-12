using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class RecordingSignInService : ISignInService
{
    public SignInOutcome PasswordOutcome { get; set; } = SignInOutcome.Succeeded;

    public SignInOutcome TwoFactorOutcome { get; set; } = SignInOutcome.Succeeded;

    public SignInOutcome RecoveryCodeOutcome { get; set; } = SignInOutcome.Succeeded;

    public UserAccountDto? TwoFactorUser { get; set; }

    public int SignOutCount { get; private set; }

    public List<Guid> RefreshedUsers { get; } = [];

    public bool RememberDeviceRequested { get; private set; }

    public Task<SignInOutcome> PasswordSignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(PasswordOutcome);

    public Task<SignInOutcome> TwoFactorSignInAsync(
        string code,
        bool rememberMe,
        bool rememberDevice,
        CancellationToken cancellationToken = default)
    {
        RememberDeviceRequested = rememberDevice;

        return Task.FromResult(TwoFactorOutcome);
    }

    public Task<SignInOutcome> RecoveryCodeSignInAsync(
        string recoveryCode,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(RecoveryCodeOutcome);

    public Task<UserAccountDto?> GetTwoFactorUserAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(TwoFactorUser);

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        SignOutCount++;

        return Task.CompletedTask;
    }

    public Task RefreshSignInAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        RefreshedUsers.Add(userId);

        return Task.CompletedTask;
    }
}