using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Esports;

/// <summary>Новость портала.</summary>
public class NewsArticle : BaseEntity, IAuditableEntity
{
    private NewsArticle()
    {
    }

    public Slug Slug { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string? Summary { get; private set; }

    /// <summary>Текст в HTML. Очищается HtmlSanitizer на входе.</summary>
    public string BodyHtml { get; private set; } = null!;

    public string? CoverPath { get; private set; }

    public Guid AuthorId { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public bool IsPublished { get; private set; }

    public int ViewCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static NewsArticle Draft(
        Slug slug,
        string title,
        string bodyHtml,
        Guid authorId,
        string? summary = null,
        string? coverPath = null)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(bodyHtml);

        InvariantViolationException.ThrowIf(
            authorId == Guid.Empty,
            "news.author_required",
            "У новости обязан быть автор.");

        return new NewsArticle
        {
            Slug = slug,
            Title = title.Trim(),
            BodyHtml = bodyHtml,
            AuthorId = authorId,
            Summary = Normalize(summary),
            CoverPath = Normalize(coverPath),
        };
    }

    public void UpdateContent(string title, string bodyHtml, string? summary, string? coverPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(bodyHtml);

        Title = title.Trim();
        BodyHtml = bodyHtml;
        Summary = Normalize(summary);
        CoverPath = Normalize(coverPath);
    }

    public void Publish(DateTimeOffset publishedAt)
    {
        IsPublished = true;
        PublishedAt = publishedAt;
    }

    public void Unpublish()
    {
        IsPublished = false;
        PublishedAt = null;
    }

    public void RegisterView() => ViewCount++;

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}