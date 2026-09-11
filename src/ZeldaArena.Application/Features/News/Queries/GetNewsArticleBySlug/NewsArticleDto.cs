namespace ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;

/// <summary>
/// Новость для её страницы. <see cref="BodyHtml"/> — второе из двух полей, выводимых
/// через <c>Html.Raw</c>: он проходит HtmlSanitizer на входе (docs/SPEC.md §15).
/// </summary>
public sealed record NewsArticleDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Summary { get; init; }

    public string BodyHtml { get; init; } = string.Empty;

    public string? CoverPath { get; init; }

    public DateTimeOffset PublishedAt { get; init; }

    public int ViewCount { get; init; }
}