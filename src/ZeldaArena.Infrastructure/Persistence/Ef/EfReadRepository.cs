using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

public sealed class EfReadRepository<TEntity>(AppDbContext context) : IReadRepository<TEntity>
    where TEntity : BaseEntity
{
    public IQueryable<TEntity> Query() => context.Set<TEntity>().AsNoTracking();

    public Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        Query().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
}