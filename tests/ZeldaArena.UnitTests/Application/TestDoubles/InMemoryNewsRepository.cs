using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Подмена <see cref="INewsRepository"/>, повторяющая семантику рабочих реализаций:
/// наружу отдаются только опубликованные статьи, свежие впереди. Это не мок:
/// хендлер проверяется по результату выборки, а не по факту вызова.
/// </summary>
internal sealed class InMemoryNewsRepository(IEnumerable<NewsArticle> articles) : INewsRepository
{
    private readonly List<NewsArticle> _articles = [.. articles];

    public Task<PagedResult<NewsArticle>> GetPublishedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _articles.Where(article => article.IsPublished);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = search.Trim();

            query = query.Where(article =>
                article.Title.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = query.OrderByDescending(article => article.PublishedAt).ToList();

        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<NewsArticle>(items, page, pageSize, ordered.Count));
    }

    public Task<NewsArticle?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(_articles.Find(article =>
            string.Equals(article.Slug.Value, slug, StringComparison.Ordinal)));

    public Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_articles.Find(article => article.Id == id));

    public Task AddAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        _articles.Add(article);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(NewsArticle article, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoveAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        _articles.Remove(article);

        return Task.CompletedTask;
    }
}