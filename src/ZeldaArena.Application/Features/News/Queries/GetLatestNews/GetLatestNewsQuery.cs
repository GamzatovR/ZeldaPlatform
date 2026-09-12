using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.News.Queries.GetLatestNews;

public sealed record GetLatestNewsQuery(int Count = 3) : IQuery<IReadOnlyList<NewsListItemDto>>;