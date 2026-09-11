using ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;
using ZeldaArena.Domain.Enums;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Players;

public class GetPlayerBySlugQueryHandlerTests
{
    private readonly EsportsWorld _world = new();

    public GetPlayerBySlugQueryHandlerTests()
    {
        var knights = _world.AddTeam("Hyrule Knights", "HYR");
        var guard = _world.AddTeam("Kakariko Guard", "KAK");

        // Играл за «Стражей», перешёл в «Рыцарей».
        var link = _world.Sign(guard, "link", EsportsWorld.Now.AddDays(-200));
        guard.RemovePlayer(link.Id, EsportsWorld.Now.AddDays(-100));
        knights.AddPlayer(link.Id, PlayerRole.Attacker, EsportsWorld.Now.AddDays(-90));

        var impa = _world.Sign(guard, "impa", EsportsWorld.Now.AddDays(-200));

        var first = _world.Play(knights, guard, EsportsWorld.Now.AddDays(-3), 2, 0);
        first.AddPlayerStats(link.Id, knights.Id, 10, 2, 4, 3000, 1.40m);
        first.AddPlayerStats(impa.Id, guard.Id, 1, 1, 1, 100, 0.50m);

        var second = _world.Play(knights, guard, EsportsWorld.Now.AddDays(-1), 2, 1);
        second.AddPlayerStats(link.Id, knights.Id, 6, 4, 2, 2000, 1.00m);

        _world.Play(knights, guard, EsportsWorld.Now.AddDays(-2), 0, 2);
    }

    [Fact]
    public async Task Current_team_comes_first_in_the_history()
    {
        var result = await Handle("link");

        result.ShouldNotBeNull();
        result.Teams.Select(team => team.TeamName).ShouldBe(["Hyrule Knights", "Kakariko Guard"]);
        result.CurrentTeam.ShouldNotBeNull().TeamSlug.ShouldBe("hyrule-knights");
    }

    [Fact]
    public async Task Averages_come_from_the_players_own_stats()
    {
        var result = await Handle("link");

        result.ShouldNotBeNull();
        result.MatchesPlayed.ShouldBe(2);
        result.AverageKills.ShouldBe(8);
        result.AverageRating.ShouldBe(1.2, tolerance: 0.001);
    }

    /// <summary>Последние матчи — те, где у игрока есть статистика, а не все матчи его команды.</summary>
    [Fact]
    public async Task Recent_matches_are_the_ones_the_player_took_part_in()
    {
        var result = await Handle("link");

        result.ShouldNotBeNull();
        result.RecentMatches.Count.ShouldBe(2);
        result.RecentMatches[0].ScheduledAt.ShouldBe(EsportsWorld.Now.AddDays(-1));
    }

    [Fact]
    public async Task Player_without_stats_gets_zeros_not_an_error()
    {
        _world.AddPlayer("newbie");

        var result = await Handle("newbie");

        result.ShouldNotBeNull();
        result.MatchesPlayed.ShouldBe(0);
        result.Teams.ShouldBeEmpty();
    }

    [Fact]
    public async Task Unknown_slug_is_not_found() =>
        (await Handle("ganondorf")).ShouldBeNull();

    private Task<PlayerDetailsDto?> Handle(string slug) =>
        new GetPlayerBySlugQueryHandler(
                _world.Read(_world.Players),
                _world.Read(_world.RosterEntries),
                _world.Read(_world.Teams),
                _world.Read(_world.Stats),
                _world.Read(_world.Matches),
                new InMemoryQueryExecutor())
            .Handle(new GetPlayerBySlugQuery(slug), CancellationToken.None);
}