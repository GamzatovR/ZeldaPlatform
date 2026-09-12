using MediatR;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.News.Queries.GetLatestNews;

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