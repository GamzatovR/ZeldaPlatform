using ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Matches;

public class GetMatchDetailsQueryHandlerTests
{
    private static readonly DateTimeOffset MatchDay = EsportsWorld.Now.AddDays(-10);

    private readonly EsportsWorld _world = new();
    private readonly Team _knights;
    private readonly Team _guard;
    private readonly Match _match;

    public GetMatchDetailsQueryHandlerTests()
    {
        _knights = _world.AddTeam("Hyrule Knights", "HYR");
        _guard = _world.AddTeam("Kakariko Guard", "KAK");

        _world.Sign(_knights, "link", MatchDay.AddDays(-100));
        _world.Sign(_guard, "impa", MatchDay.AddDays(-100));

        // Ушёл после матча — в составе того матча обязан остаться.
        var veteran = _world.Sign(_knights, "rauru", MatchDay.AddDays(-100));
        _knights.RemovePlayer(veteran.Id, MatchDay.AddDays(1));

        // Пришёл после матча — задним числом в состав не попадает.
        _world.Sign(_knights, "newcomer", MatchDay.AddDays(2));

        _match = _world.Play(_knights, _guard, MatchDay, 2, 1);
        _match.AddPlayerStats(_world.Players[0].Id, _knights.Id, 10, 2, 5, 3200, 1.35m);
        _match.AddPlayerStats(_world.Players[1].Id, _guard.Id, 4, 8, 1, 1800, 0.81m);
    }

    [Fact]
    public async Task Lineup_is_taken_as_of_the_match_not_as_of_today()
    {
        var result = await Handle(_match.Id);

        result.ShouldNotBeNull();
        result.LineupA.Select(player => player.Nickname).ShouldBe(["link", "rauru"], ignoreOrder: true);
        result.LineupB.Select(player => player.Nickname).ShouldBe(["impa"]);
    }

    [Fact]
    public async Task Stats_are_split_by_team()
    {
        var result = await Handle(_match.Id);

        result.ShouldNotBeNull();
        result.StatsA.ShouldHaveSingleItem().Nickname.ShouldBe("link");
        result.StatsB.ShouldHaveSingleItem().Damage.ShouldBe(1800);
    }

    [Fact]
    public async Task Card_carries_the_score_and_both_teams()
    {
        var result = await Handle(_match.Id);

        result.ShouldNotBeNull();
        result.Card.ScoreA.ShouldBe(2);
        result.Card.TeamBId.ShouldBe(_guard.Id);
        result.StartedAt.ShouldBe(MatchDay);
    }

    [Fact]
    public async Task Unknown_match_is_not_found() =>
        (await Handle(Guid.CreateVersion7())).ShouldBeNull();

    [Theory]
    [InlineData("https://twitch.tv/zelda", true)]
    [InlineData("http://youtube.com/live", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("//evil.example/stream", false)]
    [InlineData("not a url", false)]
    public async Task Only_web_addresses_are_offered_as_stream_links(string url, bool offered)
    {
        _match.SetStreamUrl(url);

        var result = await Handle(_match.Id);

        result.ShouldNotBeNull();
        (result.StreamUrl is not null).ShouldBe(offered);
    }

    private Task<MatchDetailsDto?> Handle(Guid id)
    {
        var handler = new GetMatchDetailsQueryHandler(
            _world.Read(_world.Matches),
            _world.Read(_world.RosterEntries),
            _world.Read(_world.Players),
            _world.Read(_world.Stats),
            new InMemoryQueryExecutor());

        return handler.Handle(new GetMatchDetailsQuery(id), CancellationToken.None);
    }
}