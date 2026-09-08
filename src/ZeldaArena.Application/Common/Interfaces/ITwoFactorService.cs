using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Двухфакторная аутентификация по TOTP (docs/SPEC.md §8.2): ключ для аутентификатора,
/// включение и отключение, коды восстановления.
///
/// Алгоритм TOTP и хранение кодов остаются деталью Infrastructure — Application знает
/// только про «показать ключ», «проверить код» и «выдать коды восстановления».
/// </summary>
public interface ITwoFactorService
{
    /// <summary>
    /// Ключ и ссылка otpauth:// для QR-кода. Если ключа ещё нет, он создаётся.
    /// </summary>
    Task<Result<TwoFactorSetup>> GetSetupAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Включает 2FA, предварительно убедившись, что аутентификатор настроен верно.</summary>
    Task<Result> EnableAsync(Guid userId, string verificationCode, CancellationToken cancellationToken = default);

    Task<Result> DisableAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выдаёт новый комплект кодов восстановления, старые перестают действовать.
    /// Показать их можно только здесь и один раз — дальше Identity хранит их так,
    /// что прочитать нельзя.
    /// </summary>
    Task<Result<RecoveryCodes>> GenerateRecoveryCodesAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default);

    Task<Result<int>> CountRemainingRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}