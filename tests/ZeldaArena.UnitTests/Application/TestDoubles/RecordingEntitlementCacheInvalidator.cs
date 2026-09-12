using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class RecordingEntitlementCacheInvalidator : IEntitlementCacheInvalidator
{
    private readonly List<Guid> _users = [];

    public IReadOnlyList<Guid> InvalidatedUsers => _users;

    public int InvalidatedEverything { get; private set; }

    public Task InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _users.Add(userId);

        return Task.CompletedTask;
    }

    public Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        InvalidatedEverything++;

        return Task.CompletedTask;
    }
}