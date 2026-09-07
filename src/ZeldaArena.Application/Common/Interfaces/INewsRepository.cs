using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Единственный порт с двумя рабочими реализациями: <c>EfNewsRepository</c> поверх
/// PostgreSQL и <c>MongoNewsRepository</c> поверх MongoDB, выбор — ключом
/// <c>Persistence:NewsProvider</c> в конфигурации. Это точка расширения EP-1
/// (docs/SPEC.md §5.4), она демонстрируется на защите переключением конфига.
///
/// Поэтому здесь нет <see cref="IQueryable{T}"/>: у MongoDB нет провайдера LINQ,
/// совместимого с выражениями EF Core, и как только сюда просочится <c>IQueryable</c>,
/// вторая реализация станет невозможной, а EP-1 — недоказуемой. Все выборки
/// объявлены явными методами, обе реализации проходят один набор контрактных тестов
/// (§5.5, LSP).
/// </summary>
public interface INewsRepository
{
    Task<PagedResult<NewsArticle>> GetPublishedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<NewsArticle?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(NewsArticle article, CancellationToken cancellationToken = default);

    /// <summary>
    /// Асинхронный, хотя реализации на EF Core достаточно пометить состояние:
    /// MongoDB обновляет документ обращением к серверу, и порт обязан это допускать.
    /// </summary>
    Task UpdateAsync(NewsArticle article, CancellationToken cancellationToken = default);

    Task RemoveAsync(NewsArticle article, CancellationToken cancellationToken = default);
}