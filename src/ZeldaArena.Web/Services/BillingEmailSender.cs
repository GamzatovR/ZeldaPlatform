using System.Globalization;

using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Web.Services;

/// <summary>
/// Собирает письма биллинга и отдаёт их транспорту <see cref="IEmailSender"/>
/// (docs/SPEC.md §7.6).
///
/// Живёт в Web по той же причине, что и <see cref="AccountEmailSender"/>: текст берётся
/// из ресурсов, а суммы и даты форматируются по текущей культуре (§9.5). Application
/// видит один лишь порт <see cref="IBillingEmailSender"/>, поэтому правило зависимостей
/// §5.2 не нарушено.
///
/// Код подтверждения попадает в тело письма и больше никуда: в лог уходят только
/// адресат и тема, об этом заботится <c>SmtpEmailSender</c> (§13).
/// </summary>
public sealed class BillingEmailSender(
    IEmailSender emailSender,
    IStringLocalizer<SharedResource> localizer)
    : IBillingEmailSender
{
    public Task SendPaymentCodeAsync(
        string email,
        string? displayName,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        // Код стоит отдельной строкой и крупно: его переписывают руками,
        // а не копируют, и он не должен теряться в тексте.
        var body = Format("email.payment_code.body", code, Minutes(expiresAt));

        return SendAsync(email, displayName, "email.payment_code.subject", body, cancellationToken);
    }

    public Task SendPaymentReceiptAsync(
        string email,
        string? displayName,
        string planName,
        decimal amount,
        string currency,
        DateTimeOffset subscriptionEndsAt,
        CancellationToken cancellationToken = default)
    {
        var body = Format(
            "email.payment_receipt.body",
            planName,
            FormatMoney(amount, currency),
            FormatDate(subscriptionEndsAt));

        return SendAsync(email, displayName, "email.payment_receipt.subject", body, cancellationToken);
    }

    public Task SendSubscriptionExpiredAsync(
        string email,
        string? displayName,
        string planName,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            displayName,
            "email.subscription_expired.subject",
            Format("email.subscription_expired.body", planName),
            cancellationToken);

    /// <summary>
    /// Ссылки в письмах биллинга нет намеренно. Код вводится на той же странице,
    /// с которой началась оплата, а ссылка «подтвердить» в письме про деньги —
    /// это ровно то, чему учат не доверять.
    /// </summary>
    private Task SendAsync(
        string to,
        string? displayName,
        string subjectKey,
        string body,
        CancellationToken cancellationToken)
    {
        var html = EmailLayout.Render(
            greeting: Format("email.greeting", displayName ?? to),
            body: body,
            actionText: null,
            actionUrl: null,
            linkHint: string.Empty,
            footer: localizer["email.payment_footer"]);

        return emailSender.SendAsync(to, localizer[subjectKey], html, cancellationToken);
    }

    private static int Minutes(DateTimeOffset expiresAt) =>
        Math.Max(1, (int)Math.Round((expiresAt - DateTimeOffset.UtcNow).TotalMinutes));

    private static string FormatMoney(decimal amount, string currency) =>
        string.Create(CultureInfo.CurrentCulture, $"{amount:N2} {currency}");

    private static string FormatDate(DateTimeOffset moment) =>
        moment.ToLocalTime().ToString("d", CultureInfo.CurrentCulture);

    private string Format(string key, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, localizer[key], arguments);
}