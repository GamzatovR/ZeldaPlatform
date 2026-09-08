using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Вход и выход (docs/SPEC.md §8.2). Реализация в Infrastructure оборачивает
/// SignInManager: только он умеет выписать cookie аутентификации, и только у него
/// есть счётчик неудачных попыток для lockout.
///
/// Порт узкий по ISP (§5.5): сценарии профиля и смены пароля сюда не заглядывают.
/// </summary>
public interface ISignInService
{
    Task<SignInOutcome> PasswordSignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Второй шаг входа. Пользователь берётся из промежуточной cookie, выписанной
    /// первым шагом, — поэтому идентификатор в параметрах не нужен и передавать его
    /// с клиента было бы дырой.
    /// </summary>
    Task<SignInOutcome> TwoFactorSignInAsync(
        string code,
        bool rememberMe,
        bool rememberDevice,
        CancellationToken cancellationToken = default);

    Task<SignInOutcome> RecoveryCodeSignInAsync(
        string recoveryCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Пользователь, ожидающий второго фактора. <c>null</c> означает, что промежуточная
    /// сессия истекла и вход нужно начинать с пароля.
    /// </summary>
    Task<UserAccountDto?> GetTwoFactorUserAsync(CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Перевыписывает cookie текущему пользователю. Нужна после действий, меняющих
    /// стамп безопасности, — иначе тот, кто только что сменил пароль или включил 2FA,
    /// разлогинил бы сам себя вместе с остальными сессиями.
    /// </summary>
    Task RefreshSignInAsync(Guid userId, CancellationToken cancellationToken = default);
}