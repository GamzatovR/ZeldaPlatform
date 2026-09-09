using Microsoft.Extensions.Caching.Memory;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Кэш прав на платные функции: TTL пять минут по docs/SPEC.md §7.3.
///
/// Права спрашивают на каждом защищённом действии и на каждом &lt;feature-gate&gt;
/// в разметке — без кэша страница с тремя блоками давала бы три обращения к базе.
///
/// «Сбросить всем» сделано счётчиком поколения в ключе, а не обходом записей:
/// <see cref="IMemoryCache"/> не умеет перечислять ключи, а держать рядом список
/// значило бы вести второй, расходящийся с ним источник истины. Смена поколения
/// делает все прежние ключи недостижимыми, и старые записи уходят по TTL сами.
/// </summary>
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