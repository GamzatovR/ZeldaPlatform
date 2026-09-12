using Microsoft.AspNetCore.Authentication.Cookies;

using ZeldaArena.Web.Areas.Api.Controllers;

namespace ZeldaArena.Web.Areas.Api;

public static class ApiRequest
{
    public static bool Is(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Path.StartsWithSegments(ApiControllerBase.PathPrefix, StringComparison.OrdinalIgnoreCase);
    }

    public static void AnswerApiWithStatusCodes(this CookieAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Events.OnRedirectToLogin = context =>
            StatusOrRedirect(context.Request, context.Response, context.RedirectUri, StatusCodes.Status401Unauthorized);

        options.Events.OnRedirectToAccessDenied = context =>
            StatusOrRedirect(context.Request, context.Response, context.RedirectUri, StatusCodes.Status403Forbidden);
    }

    private static Task StatusOrRedirect(HttpRequest request, HttpResponse response, string redirectUri, int statusCode)
    {
        if (Is(request))
        {
            response.StatusCode = statusCode;
        }
        else
        {
            response.Redirect(redirectUri);
        }

        return Task.CompletedTask;
    }
}