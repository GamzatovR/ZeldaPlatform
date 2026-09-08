using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Второй фактор в памяти. Настоящий TOTP не воспроизводится: годным считается
/// заранее оговорённый код. Тестам хендлеров важна не арифметика алгоритма,
/// а то, что происходит при верном и неверном коде.
/// </summary>
internal sealed class StubTwoFactorService : ITwoFactorService
{
    public const string ValidCode = "123456";

    private readonly HashSet<Guid> _enabled = [];

    public int GeneratedCodeCount { get; private set; }

    public int RemainingRecoveryCodes { get; set; }

    public bool WasKeyReset { get; private set; }

    public void Enable(Guid userId)
    {
        _enabled.Add(userId);
        RemainingRecoveryCodes = 10;
    }

    public Task<Result<TwoFactorSetup>> GetSetupAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(
            new TwoFactorSetup("abcd efgh ijkl", $"otpauth://totp/ZeldaArena:{userId}")));

    public Task<Result> EnableAsync(
        Guid userId,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        if (verificationCode != ValidCode)
        {
            return Task.FromResult(Result.Failure(AccountErrors.TwoFactorCodeInvalid));
        }

        _enabled.Add(userId);

        return Task.FromResult(Result.Success());
    }

    public Task<Result> DisableAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _enabled.Remove(userId);
        WasKeyReset = true;
        RemainingRecoveryCodes = 0;

        return Task.FromResult(Result.Success());
    }

    public Task<Result<RecoveryCodes>> GenerateRecoveryCodesAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (!_enabled.Contains(userId))
        {
            return Task.FromResult(Result.Failure<RecoveryCodes>(AccountErrors.TwoFactorNotEnabled));
        }

        GeneratedCodeCount = count;
        RemainingRecoveryCodes = count;

        var codes = Enumerable.Range(1, count).Select(index => $"code-{index:00}").ToArray();

        return Task.FromResult(Result.Success(new RecoveryCodes(codes)));
    }

    public Task<Result<int>> CountRemainingRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(RemainingRecoveryCodes));

    public bool IsEnabled(Guid userId) => _enabled.Contains(userId);
}