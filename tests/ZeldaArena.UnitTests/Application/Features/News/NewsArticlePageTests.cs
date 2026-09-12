using ZeldaArena.Application.Features.News.Commands.RegisterNewsView;
using ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.News;

/// <summary>Страница новости и её счётчик просмотров.</summary>
public class NewsArticlePageTests
{
    private static readonly DateTimeOffset Published = new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid Author = Guid.CreateVersion7();

    private readonly NewsArticle _article;
    private readonly NewsArticle _draft;
    private readonly InMemoryNewsRepository _news;

    public NewsArticlePageTests()
    {
        _article = NewsArticle.Draft(Slug.From("hyrule-open-anons"), "Анонс Hyrule Open", "<p>Скоро</p>", Author, "Кратко");
        _article.Publish(Published);

        _draft = NewsArticle.Draft(Slug.From("chernovik"), "Черновик", "<p>…</p>", Author);

        _news = new InMemoryNewsRepository([_article, _draft]);
    }

    [Fact]
    public async Task Published_article_opens_with_its_body()
    {
        var result = await Get("hyrule-open-anons");

        result.ShouldNotBeNull();
        result.BodyHtml.ShouldBe("<p>Скоро</p>");
        result.PublishedAt.ShouldBe(Published);
    }

    /// <summary>Черновик по прямой ссылке не открывается: для читателя его нет.</summary>
    [Fact]
    public async Task Draft_is_not_found_even_by_a_direct_link() =>
        (await Get("chernovik")).ShouldBeNull();

    [Fact]
    public async Task Unknown_article_is_not_found() =>
        (await Get("no-such-news")).ShouldBeNull();

    [Fact]
    public async Task Slug_from_the_address_is_matched_case_insensitively() =>
        (await Get("Hyrule-Open-Anons")).ShouldNotBeNull();

    [Fact]
    public async Task View_is_counted_and_saved()
    {
        var unitOfWork = new RecordingUnitOfWork();

        var result = await new RegisterNewsViewCommandHandler(_news, unitOfWork)
            .Handle(new RegisterNewsViewCommand(_article.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _article.ViewCount.ShouldBe(1);
        unitOfWork.SaveChangesCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Draft_views_are_not_counted()
    {
        await new RegisterNewsViewCommandHandler(_news, new RecordingUnitOfWork())
            .Handle(new RegisterNewsViewCommand(_draft.Id), CancellationToken.None);

        _draft.ViewCount.ShouldBe(0);
    }

    private Task<NewsArticleDto?> Get(string slug) =>
        new GetNewsArticleBySlugQueryHandler(_news).Handle(new GetNewsArticleBySlugQuery(slug), CancellationToken.None);
}