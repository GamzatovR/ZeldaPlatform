using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

/// <summary>
/// Реализация хранилища новостей на PostgreSQL — та, что работает при
/// <c>Persistence:NewsProvider=Ef</c>. Вторая, на MongoDB, появляется в Фазе 10
/// и доказывает EP-1 (docs/SPEC.md §5.4): ни Application, ни Web при переключении
/// не меняются.
/// </summary>
public sealed class EfNewsRepository(AppDbContext context) : INewsRepository
{
    public async Task<PagedResult<NewsArticle>> GetPublishedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var query = context.NewsArticles
            .AsNoTracking()
            .Where(article => article.IsPublished);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";

            query = query.Where(article =>
                EF.Functions.ILike(article.Title, pattern)
                || (article.Summary != null && EF.Functions.ILike(article.Summary, pattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(article => article.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<NewsArticle>(items, page, pageSize, totalCount);
    }

    public Task<NewsArticle?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return context.NewsArticles
            .FirstOrDefaultAsync(article => article.Slug.Value == slug, cancellationToken);
    }

    public Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.NewsArticles.FirstOrDefaultAsync(article => article.Id == id, cancellationToken);

    public async Task AddAsync(NewsArticle article, CancellationToken cancellationToken = default) =>
        await context.NewsArticles.AddAsync(article, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Сущность отслеживается контекстом, поэтому достаточно вернуть завершённую задачу.
    /// Асинхронность здесь ради второй реализации: MongoDB обновляет документ
    /// обращением к серверу.
    /// </summary>
    public Task UpdateAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(article);

        context.NewsArticles.Update(article);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(article);

        context.NewsArticles.Remove(article);

        return Task.CompletedTask;
    }
}