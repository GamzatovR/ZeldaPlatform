using ZeldaArena.Application.Features.News.Queries.GetLatestNews;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.News;

public class GetLatestNewsQueryHandlerTests
{
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly DateTimeOffset Season = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Returns_published_articles_newest_first()
    {
        var result = await Handle(new GetLatestNewsQuery());

        result.Select(article => article.Title)
            .ShouldBe(["Финал сезона", "Новый состав", "Расписание плей-офф"]);
    }

    [Fact]
    public async Task Drafts_never_reach_the_feed()
    {
        var result = await Handle(new GetLatestNewsQuery(Count: 12));

        result.ShouldNotContain(article => article.Title == "Черновик");
    }

    [Fact]
    public async Task Count_limits_the_feed()
    {
        var result = await Handle(new GetLatestNewsQuery(Count: 2));

        result.Count.ShouldBe(2);
    }

    [Fact]
    public void The_list_item_carries_no_html_body() =>
        typeof(NewsListItemDto)
            .GetProperties()
            .Select(property => property.Name)
            .ShouldNotContain("BodyHtml");

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(GetLatestNewsQueryValidator.MaxCount + 1)]
    public void Validator_rejects_a_count_outside_the_allowed_range(int count) =>
        new GetLatestNewsQueryValidator()
            .Validate(new GetLatestNewsQuery(count))
            .IsValid.ShouldBeFalse();

    private static async Task<IReadOnlyList<NewsListItemDto>> Handle(GetLatestNewsQuery query)
    {
        var handler = new GetLatestNewsQueryHandler(new InMemoryNewsRepository(Feed()));

        return await handler.Handle(query, CancellationToken.None);
    }

    private static IReadOnlyList<NewsArticle> Feed()
    {
        var playoffs = Publish("playoff-schedule", "Расписание плей-офф", daysAgo: 9);
        var roster = Publish("new-roster", "Новый состав", daysAgo: 4);
        var final = Publish("season-final", "Финал сезона", daysAgo: 1);

        var draft = NewsArticle.Draft(
            Slug.From("draft"),
            "Черновик",
            "<p>Ещё не готово.</p>",
            Author);

        return [playoffs, roster, final, draft];
    }

    private static NewsArticle Publish(string slug, string title, int daysAgo)
    {
        var article = NewsArticle.Draft(
            Slug.From(slug),
            title,
            $"<p>{title}</p>",
            Author,
            summary: title);

        article.Publish(Season.AddDays(-daysAgo));

        return article;
    }
}