using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

/// <summary>
/// Записи поверх <see cref="AppDbContext"/>. Сущность возвращается отслеживаемой,
/// поэтому изменения, сделанные её методами, попадут в базу при сохранении —
/// отдельный вызов Update не нужен и намеренно не предусмотрен портом.
/// </summary>
public sealed class EfRepository<TEntity>(AppDbContext context) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Set<TEntity>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await context.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);

    public void Remove(TEntity entity) => context.Set<TEntity>().Remove(entity);
}