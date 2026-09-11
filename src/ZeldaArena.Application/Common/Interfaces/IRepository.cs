using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Порт записи. Сущность возвращается отслеживаемой, поэтому метода <c>Update</c> нет:
/// изменения фиксирует <see cref="IUnitOfWork.SaveChangesAsync"/>, а отдельный
/// <c>Update</c> только приглашал бы присоединять объекты, собранные из HTTP-запроса, —
/// это прямо запрещено (docs/SPEC.md §15: доменные сущности не биндятся из запроса).
/// </summary>
public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Несколько сущностей одним запросом. Оформление заказа и слияние корзин меняют
    /// остаток у каждого товара позиции, и загрузка по одному давала бы запрос на строку
    /// (docs/SPEC.md §16, проверка на N+1). Отсутствующие идентификаторы пропускаются.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Remove(TEntity entity);
}