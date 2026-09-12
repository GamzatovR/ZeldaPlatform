namespace ZeldaArena.Application.Features.News.Queries.GetLatestNews;

public sealed record NewsListItemDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Summary { get; init; }

    public string? CoverPath { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }
}