using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IQueryExecutor
{
    Task<IReadOnlyList<T>> ToListAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<decimal> SumAsync(
        IQueryable<decimal> query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}