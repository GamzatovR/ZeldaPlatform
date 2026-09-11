using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class StubCurrentUserService : ICurrentUserService
{
    /// <summary>Изменяемый: сценарии магазина проверяют, что происходит при входе гостя.</summary>
    public Guid? UserId { get; set; }

    public string? UserName { get; init; }

    public bool IsAuthenticated => UserId.HasValue;

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public string? CorrelationId { get; init; }

    public bool IsInRole(string role) => false;
}