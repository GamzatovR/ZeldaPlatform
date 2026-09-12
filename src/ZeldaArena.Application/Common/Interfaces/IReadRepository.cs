using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IReadRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>Запрос без отслеживания изменений: чтения не грузят change tracker.</summary>
    IQueryable<TEntity> Query();

    Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}