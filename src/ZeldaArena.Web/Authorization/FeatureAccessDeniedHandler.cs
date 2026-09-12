using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

using ZeldaArena.Web.Areas.Api;

namespace ZeldaArena.Web.Authorization;

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

    private static bool PrefersJson(HttpRequest request) =>
        request.Headers.Accept.Any(value =>
            value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);
}