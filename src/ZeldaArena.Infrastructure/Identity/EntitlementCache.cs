using Microsoft.Extensions.Caching.Memory;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Кэш прав на платные функции.</summary>
public sealed class EntitlementCache(IMemoryCache cache) : IEntitlementCacheInvalidator
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    private long _generation;

    public async Task<EntitlementSet> GetOrCreateAsync(
        Guid userId,
        Func<Task<EntitlementSet>> factory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        cancellationToken.ThrowIfCancellationRequested();

        var key = KeyFor(userId, Interlocked.Read(ref _generation));

        if (cache.TryGetValue<EntitlementSet>(key, out var cached) && cached is not null)
        {
            return cached;
        }

        var entitlements = await factory().ConfigureAwait(false);

        cache.Set(key, entitlements, Lifetime);

        return entitlements;
    }

    public Task InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        cache.Remove(KeyFor(userId, Interlocked.Read(ref _generation)));

        return Task.CompletedTask;
    }

    public Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Interlocked.Increment(ref _generation);

        return Task.CompletedTask;
    }

    private static string KeyFor(Guid userId, long generation) =>
        $"entitlements:{generation}:{userId}";
}