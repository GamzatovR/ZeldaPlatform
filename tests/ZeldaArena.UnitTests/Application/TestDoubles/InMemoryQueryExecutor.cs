using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class InMemoryQueryExecutor : IQueryExecutor
{
    public Task<IReadOnlyList<T>> ToListAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>([.. query]);

    public Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(query.FirstOrDefault());

    public Task<int> CountAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(query.Count());

    public Task<bool> AnyAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(query.Any());

    public Task<decimal> SumAsync(
        IQueryable<decimal> query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(query.Sum());

    public Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        return Task.FromResult(new PagedResult<T>(items, page, pageSize, totalCount));
    }
}