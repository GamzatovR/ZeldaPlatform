using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Учётные записи: заведение, подтверждение почты, пароли, смена адреса, роли, профиль
/// (docs/SPEC.md §8.2). Реализация в Infrastructure оборачивает UserManager.
///
/// Порт намеренно не отдаёт наружу ни ApplicationUser, ни IdentityResult: Application
/// не должен знать об ASP.NET Core (правило 2 §5.2), поэтому наружу идут UserAccountDto
/// и общий для решения Result.
///
/// Разделение с ISignInService и ITwoFactorService — это ISP из §5.5: сценарию смены
/// пароля незачем видеть методы второго фактора, а тесту входа — подставлять заглушки
/// для работы с профилем.
/// </summary>
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

    /// <summary>
    /// Токены возвращаются сырыми: кодирование для URL — забота слоя, который строит
    /// ссылку, а не порта.
    /// </summary>
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

    /// <summary>Меняет пароль с проверкой текущего — без него смена запрещена (§8.2).</summary>
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

    /// <summary>
    /// Отмечает удачный вход в LastLoginAt. Вызывается хендлером явно, а не прячется
    /// внутри реализации входа: это наблюдаемое поведение сценария, и в тесте оно
    /// должно быть видно.
    /// </summary>
    Task<Result> RecordSignInAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет стамп безопасности: все прочие сессии пользователя перестают
    /// действовать в пределах SecurityStampValidationInterval (§8.2).
    /// </summary>
    Task<Result> InvalidateOtherSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}