namespace ZeldaArena.Application.Common.Interfaces;

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