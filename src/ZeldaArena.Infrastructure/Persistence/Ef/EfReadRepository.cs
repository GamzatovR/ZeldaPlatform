using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

/// <summary>
/// Чтения поверх <see cref="AppDbContext"/>. Запрос всегда без отслеживания изменений
/// (docs/SPEC.md §16): страница списка не должна наполнять change tracker объектами,
/// которые никто не собирается менять.
/// </summary>
public sealed class EfReadRepository<TEntity>(AppDbContext context) : IReadRepository<TEntity>
    where TEntity : BaseEntity
{
    public IQueryable<TEntity> Query() => context.Set<TEntity>().AsNoTracking();

    public Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        Query().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
}