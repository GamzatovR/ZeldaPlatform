using MediatR;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;

/// <summary>
/// Через <see cref="INewsRepository"/>, а не <c>IReadRepository</c>: у новостей два
/// хранилища, и страница новости обязана работать на обоих (EP-1, docs/SPEC.md §5.4).
/// </summary>
public sealed class GetNewsArticleBySlugQueryHandler(INewsRepository news)
    : IRequestHandler<GetNewsArticleBySlugQuery, NewsArticleDto?>
{
    public async Task<NewsArticleDto?> Handle(GetNewsArticleBySlugQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var article = await news.GetBySlugAsync(request.Slug.Trim().ToLowerInvariant(), cancellationToken)
            .ConfigureAwait(false);

        if (article is not { IsPublished: true, PublishedAt: { } publishedAt })
        {
            return null;
        }

        return new NewsArticleDto
        {
            Id = article.Id,
            Slug = article.Slug.Value,
            Title = article.Title,
            Summary = article.Summary,
            BodyHtml = article.BodyHtml,
            CoverPath = article.CoverPath,
            PublishedAt = publishedAt,
            ViewCount = article.ViewCount,
        };
    }
}