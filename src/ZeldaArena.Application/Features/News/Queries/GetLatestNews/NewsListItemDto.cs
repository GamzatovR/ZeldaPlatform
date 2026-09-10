namespace ZeldaArena.Application.Features.News.Queries.GetLatestNews;

/// <summary>
/// Карточка новости в ленте. <c>BodyHtml</c> сюда не попадает сознательно:
/// в списке он не нужен, а любой его вывод требует Html.Raw и санитизации
/// на входе (docs/SPEC.md §15).
/// </summary>
public sealed record NewsListItemDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Summary { get; init; }

    public string? CoverPath { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }
}