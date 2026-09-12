using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Web.Middleware;

namespace ZeldaArena.Web.Services;

public sealed class CookieGuestCartIdentity(IHttpContextAccessor httpContextAccessor) : IGuestCartIdentity
{
    public Guid? AnonymousId =>
        httpContextAccessor.HttpContext?.Items[CartCookieMiddleware.AnonymousIdKey] as Guid?;
}