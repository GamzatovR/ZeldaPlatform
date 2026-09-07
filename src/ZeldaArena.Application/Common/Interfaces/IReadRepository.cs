using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Порт чтения. Отдельно от <see cref="IRepository{TEntity}"/> по ISP (docs/SPEC.md §5.5):
/// хендлеру запроса нечего делать с методами записи, и наоборот.
///
/// <see cref="Query"/> возвращает <see cref="IQueryable{T}"/> — это System.Linq, а не EF Core,
/// поэтому правило 2 §5.2 не нарушено. Так хендлер собирает фильтр, сортировку по whitelist
/// и проекцию сразу в DTO одним выражением, которое целиком уезжает в SQL (§16).
/// Материализацию делает <see cref="IQueryExecutor"/>: асинхронные операторы живут в EF Core,
/// которого здесь быть не может.
/// </summary>
public interface IReadRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>Запрос без отслеживания изменений: чтения не грузят change tracker (§16).</summary>
    IQueryable<TEntity> Query();

    Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}