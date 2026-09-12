using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Асинхронная материализация запроса. Порт сверх списка docs/SPEC.md §5.3 и вот почему:
/// собрать <see cref="IQueryable{T}"/> в Application можно (это System.Linq), а выполнить
/// его асинхронно — нет, потому что <c>ToListAsync</c> и <c>CountAsync</c> объявлены
/// в EF Core, а он в Application запрещён правилом 2 §5.2.
///
/// Без этого порта пришлось бы либо тащить EF Core в Application, либо заводить в
/// репозитории по методу на каждый список — и единый механизм фильтрации из §10.2
/// рассыпался бы на копии. Обоснование целиком — в docs/adr/ADR-0004.
/// </summary>
public interface IQueryExecutor
{
    Task<IReadOnlyList<T>> ToListAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Сумма, посчитанная базой. На пустой выборке — ноль: выручка за период без
    /// платежей — это ноль рублей, а не отсутствие ответа.
    /// </summary>
    Task<decimal> SumAsync(
        IQueryable<decimal> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Считает общее число записей и берёт одну страницу. Два обращения к базе вместо
    /// одного — сознательная плата за <c>TotalCount</c>, без которого не построить
    /// пагинацию из §10.3.
    /// </summary>
    Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}