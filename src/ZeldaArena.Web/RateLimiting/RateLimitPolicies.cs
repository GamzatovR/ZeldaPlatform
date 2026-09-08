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
/// можно будет в Фазе 8, когда формы аккаунта получат AJAX-эндпоинты.
///
/// Оплата, комментарии и чат получат свои политики в Фазах 4 и 10.
/// </summary>
public static class RateLimitPolicies
{
    public const string SignIn = "account-sign-in";
    public const string Register = "account-register";
    public const string PasswordRecovery = "account-password-recovery";
    public const string EmailDelivery = "account-email-delivery";

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