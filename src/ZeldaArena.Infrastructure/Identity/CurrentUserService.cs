using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Текущий пользователь из <c>HttpContext</c>. До Фазы 3, пока Identity не подключён,
/// честно отдаёт неаутентифицированного пользователя — это не заглушка, а верный ответ
/// для приложения без входа.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    /// <summary>
    /// Ключ, под которым CorrelationIdMiddleware Фазы 11 кладёт идентификатор запроса
    /// (docs/SPEC.md §14.1). До её появления используется TraceIdentifier — он тоже
    /// уникален в пределах запроса, просто не сквозной.
    /// </summary>
    public const string CorrelationIdItemKey = "CorrelationId";

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;

    public string? UserName => Principal?.Identity?.Name;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.Items.TryGetValue(CorrelationIdItemKey, out var value) == true
            ? value as string
            : httpContextAccessor.HttpContext?.TraceIdentifier;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsInRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return Principal?.IsInRole(role) ?? false;
    }
}