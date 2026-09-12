using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class InMemoryReadRepository<TEntity>(IEnumerable<TEntity> entities)
    : IReadRepository<TEntity>
    where TEntity : BaseEntity
{
    public IQueryable<TEntity> Query() => entities.AsQueryable();

    public Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(entities.FirstOrDefault(entity => entity.Id == id));
}