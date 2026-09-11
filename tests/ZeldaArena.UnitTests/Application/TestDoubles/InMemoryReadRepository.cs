using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Порт чтения над последовательностью в памяти. Читает источник при каждом запросе,
/// а не снимок на момент создания — как база: запись, сохранённая одним шагом сценария,
/// видна следующему шагу того же сценария.
/// </summary>
internal sealed class InMemoryReadRepository<TEntity>(IEnumerable<TEntity> entities)
    : IReadRepository<TEntity>
    where TEntity : BaseEntity
{
    public IQueryable<TEntity> Query() => entities.AsQueryable();

    public Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(entities.FirstOrDefault(entity => entity.Id == id));
}