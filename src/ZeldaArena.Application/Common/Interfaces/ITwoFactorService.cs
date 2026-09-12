using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

public interface ITwoFactorService
{
    Task<Result<TwoFactorSetup>> GetSetupAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Включает 2FA, предварительно убедившись, что аутентификатор настроен верно.</summary>
    Task<Result> EnableAsync(Guid userId, string verificationCode, CancellationToken cancellationToken = default);

    Task<Result> DisableAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<RecoveryCodes>> GenerateRecoveryCodesAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default);

    Task<Result<int>> CountRemainingRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}