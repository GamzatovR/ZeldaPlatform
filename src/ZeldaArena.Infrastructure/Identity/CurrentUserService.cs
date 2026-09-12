using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Текущий пользователь из HttpContext.</summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
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