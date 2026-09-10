using MediatR;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.News.Queries.GetLatestNews;

/// <summary>
/// Идёт через <c>INewsRepository</c>, а не через <c>IReadRepository</c>: у новостей
/// две рабочие реализации хранилища, и вторая — на MongoDB (EP-1, docs/SPEC.md §5.4).
/// Собери здесь <c>IQueryable</c> — и переключение <c>Persistence:NewsProvider</c>
/// перестало бы работать, а точка расширения стала бы недоказуемой.
///
/// Нового метода порту не понадобилось: первая страница <c>GetPublishedAsync</c>
/// и есть свежие новости, отсортированные по дате публикации.
/// </summary>
public sealed class GetLatestNewsQueryHandler(INewsRepository news)
    : IRequestHandler<GetLatestNewsQuery, IReadOnlyList<NewsListItemDto>>
{
    public async Task<IReadOnlyList<NewsListItemDto>> Handle(
        GetLatestNewsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = await news
            .GetPublishedAsync(page: 1, pageSize: request.Count, search: null, cancellationToken)
            .ConfigureAwait(false);

        return
        [
            .. page.Items.Select(article => new NewsListItemDto
            {
                Id = article.Id,
                Slug = article.Slug.Value,
                Title = article.Title,
                Summary = article.Summary,
                CoverPath = article.CoverPath,
                PublishedAt = article.PublishedAt,
            })
        ];
    }
}