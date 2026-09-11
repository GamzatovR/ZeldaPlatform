using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

using ZeldaArena.Web.Areas.Api;

namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Отказ из-за отсутствующей платной функции — это не «вам сюда нельзя», а «нужна
/// подписка», и отвечать на него голым 403 неправильно: docs/SPEC.md §7.3 требует
/// увести на тарифы и объяснить, какой из них открывает функцию.
///
/// Отличить один случай от другого можно только здесь: к моменту, когда сработала бы
/// страница отказа в доступе, известен лишь код ответа, а какое требование не прошло —
/// уже нет.
///
/// Всё остальное — обычные политики §8.1, неаутентифицированный пользователь, запросы
/// к API — отдаётся обработчику по умолчанию.
/// </summary>
public sealed class FeatureAccessDeniedHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(authorizeResult);

        var featureCode = MissingFeatureCode(authorizeResult);

        // Аноним сначала входит: предлагать ему тариф, не спросив, кто он,
        // значит потерять пользователя, у которого подписка уже есть.
        if (featureCode is null
            || context.User.Identity?.IsAuthenticated != true
            || PrefersJson(context.Request)
            || ApiRequest.Is(context.Request))
        {
            return _default.HandleAsync(next, context, policy, authorizeResult);
        }

        var returnUrl = context.Request.GetEncodedPathAndQuery();

        context.Response.Redirect(QueryHelpers.AddQueryString(
            FeaturePolicy.SubscriptionPath,
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [FeaturePolicy.RequiredFeatureParameter] = featureCode,
                ["returnUrl"] = returnUrl,
            }));

        return Task.CompletedTask;
    }

    private static string? MissingFeatureCode(PolicyAuthorizationResult authorizeResult) =>
        authorizeResult.Challenged || authorizeResult.Succeeded
            ? null
            : authorizeResult.AuthorizationFailure?.FailedRequirements
                .OfType<FeatureRequirement>()
                .FirstOrDefault()
                ?.FeatureCode;

    /// <summary>
    /// Клиент, ждущий JSON, и любой запрос к Areas/Api получают код ответа: редирект
    /// на HTML-страницу тарифов сломал бы вызывающий их fetch (§10.1).
    /// </summary>
    private static bool PrefersJson(HttpRequest request) =>
        request.Headers.Accept.Any(value =>
            value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);
}