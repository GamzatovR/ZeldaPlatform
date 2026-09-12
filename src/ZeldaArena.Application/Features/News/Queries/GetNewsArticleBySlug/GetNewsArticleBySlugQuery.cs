using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;

public sealed record GetNewsArticleBySlugQuery(string Slug) : IQuery<NewsArticleDto?>;