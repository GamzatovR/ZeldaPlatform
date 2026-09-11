using MediatR;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Abstractions;

using ZeldaArena.Application.Common.Exceptions;
using ZeldaArena.Application.Features.Carts.Commands.MergeGuestCart;

namespace ZeldaArena.Web.Middleware;

/// <summary>
/// Гостевая корзина (docs/SPEC.md §14.1, п. 4): выдаёт и проверяет подписанный
/// <c>AnonymousId</c>, а на первом запросе после входа вливает гостевую корзину
/// в пользовательскую.
///
/// Кука подписана Data Protection, а не хранит голый Guid: подставив чужой
/// идентификатор, можно было бы читать и менять чужую корзину. Подделанная или
/// испорченная кука просто не принимается — гость получает новую, пустую корзину.
///
/// Слияние делается здесь, а не в сценариях входа: путей входа три (пароль, второй
/// фактор, код восстановления), а место, где одновременно известны и пользователь,
/// и гостевая кука, — одно.
/// </summary>
public sealed class CartCookieMiddleware(
    RequestDelegate next,
    IDataProtectionProvider dataProtection,
    ILogger<CartCookieMiddleware> logger)
{
    public const string CookieName = "za_cart";

    /// <summary>Ключ в <see cref="HttpContext.Items"/>, откуда его читает <c>CookieGuestCartIdentity</c>.</summary>
    public static readonly object AnonymousIdKey = new();

    private const string ProtectorPurpose = "ZeldaArena.Cart.AnonymousId.v1";

    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    public async Task InvokeAsync(HttpContext context, ISender sender)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(sender);

        // Корзина нужна только страницам MVC и Razor Pages. Статическому файлу кука
        // ни к чему, а Set-Cookie в ответе с годовым кэшем мешал бы этот ответ кэшировать.
        if (context.GetEndpoint()?.Metadata.GetMetadata<ActionDescriptor>() is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var protector = dataProtection.CreateProtector(ProtectorPurpose);
        var anonymousId = Read(context, protector);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (anonymousId is not null)
            {
                await MergeAsync(context, sender, anonymousId.Value).ConfigureAwait(false);
            }
        }
        else
        {
            anonymousId ??= Issue(context, protector);
            context.Items[AnonymousIdKey] = anonymousId.Value;
        }

        await next(context).ConfigureAwait(false);
    }

    private async Task MergeAsync(HttpContext context, ISender sender, Guid anonymousId)
    {
        context.Items[AnonymousIdKey] = anonymousId;

        try
        {
            var result = await sender.Send(new MergeGuestCartCommand(), context.RequestAborted).ConfigureAwait(false);

            if (result.IsFailure)
            {
                logger.LogWarning("Гостевая корзина не влита в корзину пользователя: {ErrorCode}", result.Error.Code);
            }
        }
        catch (ConcurrencyConflictException)
        {
            // Две вкладки после входа пришли с одной и той же гостевой кукой: корзину
            // уже влил соседний запрос, этот нашёл её удалённой. Повторять нечего.
            logger.LogInformation("Гостевую корзину уже влил параллельный запрос.");
        }

        // Кука снимается и при неудаче: гостевая корзина принадлежит сеансу, который
        // уже закончился входом, и вечные попытки влить её на каждом запросе хуже потери.
        context.Items.Remove(AnonymousIdKey);
        context.Response.Cookies.Delete(CookieName);
    }

    private static Guid? Read(HttpContext context, IDataProtector protector)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var value) || string.IsNullOrEmpty(value))
        {
            return null;
        }

        try
        {
            return Guid.TryParse(protector.Unprotect(value), out var id) && id != Guid.Empty ? id : null;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            // Подпись не сошлась: кука подделана или выписана другим ключом.
            return null;
        }
    }

    /// <summary>
    /// Кука выдаётся на первом визите, а строка корзины в базе — только при первом
    /// добавлении товара (<c>CartLocator.GetOrCreateAsync</c>).
    /// </summary>
    private static Guid Issue(HttpContext context, IDataProtector protector)
    {
        var id = Guid.CreateVersion7();

        context.Response.Cookies.Append(CookieName, protector.Protect(id.ToString("N")), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            // Как у cookie входа при RequireSecureCookie=false: по HTTPS — только Secure,
            // по голому HTTP разработки иначе кука не вернулась бы вовсе.
            Secure = context.Request.IsHttps,
            MaxAge = Lifetime,
        });

        return id;
    }
}