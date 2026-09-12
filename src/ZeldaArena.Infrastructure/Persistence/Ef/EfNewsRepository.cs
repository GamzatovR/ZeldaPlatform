using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

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

        // Сравнение объектом-значением, а не строкой.
        if (!Slug.TryFrom(slug, out var value) || value is null)
        {
            return Task.FromResult<NewsArticle?>(null);
        }

        return context.NewsArticles
            .FirstOrDefaultAsync(article => article.Slug == value, cancellationToken);
    }

    public Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.NewsArticles.FirstOrDefaultAsync(article => article.Id == id, cancellationToken);

    public async Task AddAsync(NewsArticle article, CancellationToken cancellationToken = default) =>
        await context.NewsArticles.AddAsync(article, cancellationToken).ConfigureAwait(false);

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