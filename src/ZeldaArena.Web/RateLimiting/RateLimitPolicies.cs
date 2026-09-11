using System.Globalization;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;

namespace ZeldaArena.Web.RateLimiting;

/// <summary>
/// Ограничение частоты запросов на формах аккаунта (docs/SPEC.md §8.2).
///
/// Зачем оно, если есть lockout. Lockout защищает одну учётную запись от подбора
/// пароля, а эти политики — сервер от перебора адресов и от рассылки писем чужими
/// руками: форма восстановления пароля без ограничения превращается в бесплатный
/// спамер.
///
/// Разделение по IP, а не по учётной записи: на момент запроса пользователь
/// не аутентифицирован, и другого устойчивого признака у нас нет.
///
/// Важно при выборе порогов: атрибут висит на странице целиком, поэтому лимит
/// расходует и открытие формы, а не только отправка. Один осмысленный заход — это
/// минимум два запроса, и пороги заданы с учётом этого. Уточнить до «только POST»
/// можно, если формы аккаунта получат AJAX-эндпоинты; в Фазе 8 они остались формами.
///
/// Чат матча получит свою политику в Фазе 10.
/// </summary>
public static class RateLimitPolicies
{
    public const string SignIn = "account-sign-in";
    public const string Register = "account-register";
    public const string PasswordRecovery = "account-password-recovery";
    public const string EmailDelivery = "account-email-delivery";

    /// <summary>Создание платежа: каждый заход шлёт письмо с кодом (docs/SPEC.md §7.6).</summary>
    public const string PaymentStart = "payment-start";

    /// <summary>Подтверждение и повторная отправка кода.</summary>
    public const string PaymentConfirm = "payment-confirm";

    /// <summary>Remote-проверка занятости адреса на форме регистрации (§10.1, сценарий 11).</summary>
    public const string EmailCheck = "account-email-check";

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    public static IServiceCollection AddPlatformRateLimiter(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Клиент должен знать, когда можно повторить: без этого заголовка
            // остаётся только гадать. Своя страница 429 придёт в Фазе 11 (§14.2).
            options.OnRejected = (context, _) =>
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)Window.TotalSeconds).ToString(CultureInfo.InvariantCulture);

                return ValueTask.CompletedTask;
            };

            // Вход ограничен мягче остальных: за пятиминутное окно человек вполне
            // может ошибиться несколько раз, а перебор пароля ловит lockout.
            AddFixedWindow(options, SignIn, permitLimit: 20);

            AddFixedWindow(options, Register, permitLimit: 10);
            AddFixedWindow(options, PasswordRecovery, permitLimit: 10);

            // Отправка писем — самая дорогая операция и самый заманчивый способ
            // использовать сервер как рассыльщик, поэтому порог самый низкий.
            AddFixedWindow(options, EmailDelivery, permitLimit: 6);

            // Оплата (§7.6). Порог низкий: каждая попытка заводит платёж и шлёт
            // письмо, а осмысленных заходов за пять минут человек делает единицы.
            AddFixedWindow(options, PaymentStart, permitLimit: 8);

            // Подтверждение кода дополняет счётчик попыток в самом платеже:
            // тот защищает один платёж от перебора, эта политика — сервер
            // от перебора по многим платежам сразу.
            AddFixedWindow(options, PaymentConfirm, permitLimit: 20);

            // Remote-проверка уходит на каждую правку поля после первой ошибки, поэтому
            // порог выше, чем у форм: человек, исправляющий опечатку, в него не упрётся,
            // а перебор адресов — упрётся.
            AddFixedWindow(options, EmailCheck, permitLimit: 60);
        });

        return services;
    }

    private static void AddFixedWindow(
        RateLimiterOptions options,
        string policyName,
        int permitLimit) =>
        options.AddPolicy(policyName, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: PartitionKey(context, policyName),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = Window,

                    // Очереди нет: держать соединение в ожидании окна хуже,
                    // чем сразу ответить 429 и дать понятное сообщение.
                    QueueLimit = 0,
                }));

    /// <summary>
    /// Ключ раздела — политика плюс адрес клиента. Запросы без адреса (такое бывает
    /// за некоторыми прокси) складываются в общее ведро: пропустить их без счёта
    /// значило бы оставить дыру.
    /// </summary>
    private static string PartitionKey(HttpContext context, string policyName) =>
        $"{policyName}:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}