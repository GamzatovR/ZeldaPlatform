using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Common.Interfaces;

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

    Task UpdateAsync(NewsArticle article, CancellationToken cancellationToken = default);

    Task RemoveAsync(NewsArticle article, CancellationToken cancellationToken = default);
}