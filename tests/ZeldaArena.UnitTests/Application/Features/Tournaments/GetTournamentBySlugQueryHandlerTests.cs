using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Tournaments;

/// <summary>
/// Названия команд-участниц здесь пустые: навигационное свойство <c>TournamentTeam.Team</c>
/// заполняет EF Core. Проверяется то, что от него не зависит, — поиск по слагу, порядок
/// участников и то, что проекция исполняется вне EF Core без падения.
/// </summary>
public class GetTournamentBySlugQueryHandlerTests
{
    private static readonly DateTimeOffset Start = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid First = Guid.CreateVersion7();
    private static readonly Guid Second = Guid.CreateVersion7();
    private static readonly Guid Third = Guid.CreateVersion7();

    [Fact]
    public async Task Tournament_is_found_by_its_slug()
    {
        var result = await Handle("summer-major");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Summer Major");
        result.PrizePoolAmount.ShouldBe(500_000m);
        result.RulesHtml.ShouldBe("<p>Bo3</p>");
    }

    [Fact]
    public async Task Slug_lookup_ignores_case_the_way_slugs_are_normalised() =>
        (await Handle("Summer-Major")).ShouldNotBeNull();

    [Theory]
    [InlineData("winter-clash")]
    [InlineData("!!!")]
    public async Task Unknown_or_unparseable_slug_is_not_found(string slug) =>
        (await Handle(slug)).ShouldBeNull();

    /// <summary>У финишировавших — места, у остальных — посев: сначала места, затем посев.</summary>
    [Fact]
    public async Task Participants_with_placement_come_first_then_by_seed()
    {
        var result = await Handle("summer-major");

        result.ShouldNotBeNull();
        result.Participants.Select(participant => participant.TeamId).ShouldBe([Third, First, Second]);
    }

    private static Task<TournamentDetailsDto?> Handle(string slug)
    {
        var tournament = Tournament.Announce(
            Slug.From("summer-major"),
            "Summer Major",
            TournamentTier.S,
            Region.Europe,
            Money.FromRubles(500_000m),
            Start,
            Start.AddDays(7),
            rulesHtml: "<p>Bo3</p>");

        tournament.AddTeam(First, seed: 1);
        tournament.AddTeam(Second, seed: 2);
        tournament.AddTeam(Third, seed: 3);
        tournament.Start();
        tournament.SetPlacement(Third, placement: 1);

        var handler = new GetTournamentBySlugQueryHandler(
            new InMemoryReadRepository<Tournament>([tournament]),
            new InMemoryQueryExecutor());

        return handler.Handle(new GetTournamentBySlugQuery(slug), CancellationToken.None);
    }
}