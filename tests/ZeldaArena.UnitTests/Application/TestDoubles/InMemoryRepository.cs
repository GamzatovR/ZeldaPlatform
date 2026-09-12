using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class InMemoryRepository<TEntity>(params TEntity[] entities) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly List<TEntity> _entities = [.. entities];

    public IReadOnlyList<TEntity> Entities => _entities;

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_entities.Find(entity => entity.Id == id));

    public Task<IReadOnlyList<TEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TEntity>>([.. _entities.Where(entity => ids.Contains(entity.Id))]);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _entities.Add(entity);

        return Task.CompletedTask;
    }

    public void Remove(TEntity entity) => _entities.Remove(entity);
}