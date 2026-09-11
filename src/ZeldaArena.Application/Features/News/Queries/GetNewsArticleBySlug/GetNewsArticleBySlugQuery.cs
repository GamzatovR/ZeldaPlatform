using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;

/// <summary>
/// Новость целиком (docs/SPEC.md §9.3, п. 11). Черновик по прямой ссылке не открывается:
/// неопубликованная новость для читателя не существует. Комментарии — Фаза 8.
/// </summary>
public sealed record GetNewsArticleBySlugQuery(string Slug) : IQuery<NewsArticleDto?>;