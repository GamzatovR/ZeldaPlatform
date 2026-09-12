using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

public sealed class EfQueryExecutor : IQueryExecutor
{
    public async Task<IReadOnlyList<T>> ToListAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return IsAsync(query)
            ? await query.ToListAsync(cancellationToken).ConfigureAwait(false)
            : [.. query];
    }

    public async Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return IsAsync(query)
            ? await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            : query.FirstOrDefault();
    }

    public async Task<int> CountAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return IsAsync(query)
            ? await query.CountAsync(cancellationToken).ConfigureAwait(false)
            : query.Count();
    }

    public async Task<bool> AnyAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return IsAsync(query)
            ? await query.AnyAsync(cancellationToken).ConfigureAwait(false)
            : query.Any();
    }

    public async Task<decimal> SumAsync(
        IQueryable<decimal> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return IsAsync(query)
            ? await query.SumAsync(cancellationToken).ConfigureAwait(false)
            : query.Sum();
    }

    public async Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var totalCount = await CountAsync(query, cancellationToken).ConfigureAwait(false);

        var items = await ToListAsync(
            query.Skip((page - 1) * pageSize).Take(pageSize),
            cancellationToken).ConfigureAwait(false);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }

    private static bool IsAsync<T>(IQueryable<T> query) => query.Provider is IAsyncQueryProvider;
}