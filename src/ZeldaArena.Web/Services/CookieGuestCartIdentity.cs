using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Web.Middleware;

namespace ZeldaArena.Web.Services;

/// <summary>
/// Идентификатор гостя, уже проверенный <see cref="CartCookieMiddleware"/>. Саму куку
/// здесь не читают: подпись проверяется в одном месте, и сценарий не может получить
/// непроверенное значение.
/// </summary>
public sealed class CookieGuestCartIdentity(IHttpContextAccessor httpContextAccessor) : IGuestCartIdentity
{
    public Guid? AnonymousId =>
        httpContextAccessor.HttpContext?.Items[CartCookieMiddleware.AnonymousIdKey] as Guid?;
}