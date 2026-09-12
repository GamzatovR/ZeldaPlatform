namespace ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;

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