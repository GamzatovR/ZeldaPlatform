using System.Globalization;

using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Web.Services;

/// <summary>
/// Собирает письма аккаунта и отдаёт их транспорту <see cref="IEmailSender"/>.
///
/// Живёт в Web, а не в Infrastructure, по двум причинам: текст письма берётся
/// из тех же ресурсов, что и текст страниц (docs/SPEC.md §9.5), а абсолютная ссылка
/// строится по таблице маршрутов, которую знает только слой представления.
/// Application видит один лишь порт <see cref="IAccountEmailSender"/>, поэтому
/// правило зависимостей §5.2 не нарушено.
///
/// Токены подставляются в ссылку и больше никуда: в лог уходит только адресат и тема,
/// об этом заботится <c>SmtpEmailSender</c> (§13).
/// </summary>
public sealed class AccountEmailSender(
    IEmailSender emailSender,
    IStringLocalizer<SharedResource> localizer,
    LinkGenerator linkGenerator,
    IHttpContextAccessor httpContextAccessor)
    : IAccountEmailSender
{
    private const string IdentityArea = "Identity";

    public Task SendEmailConfirmationAsync(
        string email,
        string? displayName,
        Guid userId,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            displayName,
            subjectKey: "email.confirm.subject",
            body: localizer["email.confirm.body"],
            actionKey: "email.confirm.action",
            actionUrl: BuildLink("/Account/ConfirmEmail", new { userId, token }),
            cancellationToken);

    public Task SendPasswordResetAsync(
        string email,
        string? displayName,
        Guid userId,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            displayName,
            subjectKey: "email.reset.subject",
            body: localizer["email.reset.body"],
            actionKey: "email.reset.action",
            actionUrl: BuildLink("/Account/ResetPassword", new { userId, token }),
            cancellationToken);

    public Task SendEmailChangeConfirmationAsync(
        string newEmail,
        string? displayName,
        Guid userId,
        string newEmailToken,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            newEmail,
            displayName,
            subjectKey: "email.change.subject",
            body: localizer["email.change.body"],
            actionKey: "email.change.action",
            actionUrl: BuildLink(
                "/Account/Manage/ConfirmEmailChange",
                new { userId, email = newEmail, token = newEmailToken }),
            cancellationToken);

    public Task SendEmailChangedNoticeAsync(
        string previousEmail,
        string? displayName,
        string newEmail,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            previousEmail,
            displayName,
            subjectKey: "email.changed_notice.subject",
            body: Format("email.changed_notice.body", newEmail),
            actionKey: null,
            actionUrl: null,
            cancellationToken);

    public Task SendPasswordChangedNoticeAsync(
        string email,
        string? displayName,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            displayName,
            subjectKey: "email.password_changed.subject",
            body: localizer["email.password_changed.body"],
            actionKey: null,
            actionUrl: null,
            cancellationToken);

    /// <summary>
    /// Одна точка сборки письма. Действие необязательно: уведомления о смене пароля
    /// и адреса ничего не предлагают нажать, они только сообщают о случившемся.
    /// </summary>
    private Task SendAsync(
        string to,
        string? displayName,
        string subjectKey,
        string body,
        string? actionKey,
        string? actionUrl,
        CancellationToken cancellationToken)
    {
        var html = EmailLayout.Render(
            greeting: Format("email.greeting", displayName ?? to),
            body: body,
            actionText: actionKey is null ? null : localizer[actionKey].Value,
            actionUrl: actionUrl,
            linkHint: localizer["email.link_hint"],
            footer: localizer["email.ignore_hint"]);

        return emailSender.SendAsync(to, localizer[subjectKey], html, cancellationToken);
    }

    private string Format(string key, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, localizer[key], arguments);

    /// <summary>
    /// Ссылка обязана быть абсолютной: относительный адрес в почтовом клиенте
    /// никуда не ведёт.
    /// </summary>
    private string BuildLink(string page, object values)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "Ссылку для письма можно построить только в контексте HTTP-запроса.");

        return linkGenerator.GetUriByPage(
            httpContext,
            page: page,
            handler: null,
            values: new RouteValueDictionary(values) { ["area"] = IdentityArea })
            ?? throw new InvalidOperationException($"Не найден маршрут страницы {page}.");
    }
}