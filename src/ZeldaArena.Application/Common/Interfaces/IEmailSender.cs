namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Отправка письма (docs/SPEC.md §5.3). Реализация появляется в Фазе 3: SMTP,
/// в разработке — контейнер MailHog, где письмо с кодом оплаты видно вживую (§7.6).
/// Порт узкий намеренно (§5.5, ISP): тема, адрес и готовый HTML — шаблонизация
/// остаётся деталью инфраструктуры.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}