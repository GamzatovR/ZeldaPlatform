using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Infrastructure.Email;

/// <summary>Отправка письма по SMTP через MailKit.</summary>
public sealed class SmtpEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SmtpEmailSender> logger)
    : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlBody);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromDisplayName, _options.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient
        {
            Timeout = (int)TimeSpan.FromSeconds(_options.TimeoutSeconds).TotalMilliseconds,
        };

        var security = _options.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(_options.Host, _options.Port, security, cancellationToken)
            .ConfigureAwait(false);

        // MailHog принимает письма без пароля, поэтому аутентификация не безусловна.
        if (!string.IsNullOrEmpty(_options.UserName))
        {
            await client
                .AuthenticateAsync(_options.UserName, _options.Password ?? string.Empty, cancellationToken)
                .ConfigureAwait(false);
        }

        await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await client.DisconnectAsync(quit: true, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Письмо «{Subject}» отправлено на {Recipient}.", subject, to);
    }
}