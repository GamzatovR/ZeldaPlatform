using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Запоминает отправленные письма. Тесты проверяют не текст — он в Web, — а сам факт:
/// ушло ли письмо, на какой адрес и с каким токеном.
/// </summary>
internal sealed class RecordingAccountEmailSender : IAccountEmailSender
{
    public List<SentLetter> Sent { get; } = [];

    /// <summary>Следующая отправка бросит это исключение: имитация недоступного SMTP.</summary>
    public Exception? FailWith { get; set; }

    public Task SendEmailConfirmationAsync(
        string email,
        string? displayName,
        Guid userId,
        string token,
        CancellationToken cancellationToken = default) =>
        Record(LetterKind.EmailConfirmation, email, userId, token);

    public Task SendPasswordResetAsync(
        string email,
        string? displayName,
        Guid userId,
        string token,
        CancellationToken cancellationToken = default) =>
        Record(LetterKind.PasswordReset, email, userId, token);

    public Task SendEmailChangeConfirmationAsync(
        string newEmail,
        string? displayName,
        Guid userId,
        string newEmailToken,
        CancellationToken cancellationToken = default) =>
        Record(LetterKind.EmailChangeConfirmation, newEmail, userId, newEmailToken);

    public Task SendEmailChangedNoticeAsync(
        string previousEmail,
        string? displayName,
        string newEmail,
        CancellationToken cancellationToken = default) =>
        Record(LetterKind.EmailChangedNotice, previousEmail, userId: null, token: null);

    public Task SendPasswordChangedNoticeAsync(
        string email,
        string? displayName,
        CancellationToken cancellationToken = default) =>
        Record(LetterKind.PasswordChangedNotice, email, userId: null, token: null);

    public SentLetter Single(LetterKind kind) => Sent.Single(letter => letter.Kind == kind);

    public bool Contains(LetterKind kind) => Sent.Exists(letter => letter.Kind == kind);

    private Task Record(LetterKind kind, string to, Guid? userId, string? token)
    {
        if (FailWith is not null)
        {
            return Task.FromException(FailWith);
        }

        Sent.Add(new SentLetter(kind, to, userId, token));

        return Task.CompletedTask;
    }

    internal enum LetterKind
    {
        EmailConfirmation,
        PasswordReset,
        EmailChangeConfirmation,
        EmailChangedNotice,
        PasswordChangedNotice,
    }

    internal sealed record SentLetter(LetterKind Kind, string To, Guid? UserId, string? Token);
}