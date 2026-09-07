using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class InMemoryReadRepository<TEntity>(IEnumerable<TEntity> entities)
    : IReadRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly List<TEntity> _entities = [.. entities];

    public IQueryable<TEntity> Query() => _entities.AsQueryable();

    public Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_entities.Find(entity => entity.Id == id));
}