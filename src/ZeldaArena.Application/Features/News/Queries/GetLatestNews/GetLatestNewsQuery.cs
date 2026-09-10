using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.News.Queries.GetLatestNews;

/// <summary>
/// Свежие опубликованные новости для ленты на главной (docs/SPEC.md §9.3, страница 1).
/// </summary>
/// <param name="Count">Сколько новостей показать. Ограничивается валидатором.</param>
public sealed record GetLatestNewsQuery(int Count = 3) : IQuery<IReadOnlyList<NewsListItemDto>>;