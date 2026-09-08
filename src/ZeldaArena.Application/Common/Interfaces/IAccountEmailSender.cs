namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Письма аккаунта (docs/SPEC.md §8.2): подтверждение адреса, сброс пароля,
/// уведомления о смене пароля и адреса.
///
/// Отдельный порт над узким <see cref="IEmailSender"/> нужен, потому что письмо —
/// это текст и ссылка, то есть представление. Хендлер обязан сказать «отправь письмо
/// с подтверждением», а не собирать HTML и абсолютный URL: ни локализации, ни таблицы
/// маршрутов в Application нет и быть не должно.
///
/// Реализация живёт в Web рядом с ресурсами и LinkGenerator — это единственный слой,
/// который знает и адреса страниц, и текущую культуру. Application видит только порт,
/// поэтому правило зависимостей §5.2 не нарушено.
///
/// Токен передаётся сюда сырым и наружу не выходит: он попадает только в ссылку внутри
/// письма и в логи не пишется (§13).
/// </summary>
public interface IAccountEmailSender
{
    Task SendEmailConfirmationAsync(
        string email,
        string? displayName,
        Guid userId,
        string token,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string email,
        string? displayName,
        Guid userId,
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>Ссылка уходит на новый адрес — подтвердить смену может только его владелец.</summary>
    Task SendEmailChangeConfirmationAsync(
        string newEmail,
        string? displayName,
        Guid userId,
        string newEmailToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомление на прежний адрес. Нужно, чтобы захват учётной записи не прошёл
    /// незамеченным: владелец узнает о смене, даже если доступ уже потерян (§8.2).
    /// </summary>
    Task SendEmailChangedNoticeAsync(
        string previousEmail,
        string? displayName,
        string newEmail,
        CancellationToken cancellationToken = default);

    Task SendPasswordChangedNoticeAsync(
        string email,
        string? displayName,
        CancellationToken cancellationToken = default);
}