using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Teams;

public class GetTeamAdvancedStatsQueryHandlerTests
{
    private static readonly Guid Subscriber = Guid.CreateVersion7();

    private readonly EsportsWorld _world = new();
    private readonly StubEntitlementService _entitlements = new StubEntitlementService()
        .Grant(Subscriber, FeatureCodes.StatsAdvanced);

    private readonly Team _knights;

    public GetTeamAdvancedStatsQueryHandlerTests()
    {
        _knights = _world.AddTeam("Hyrule Knights", "HYR");
        var guard = _world.AddTeam("Kakariko Guard", "KAK");
        var link = _world.Sign(_knights, "link", EsportsWorld.Now.AddDays(-60));
        var impa = _world.Sign(guard, "impa", EsportsWorld.Now.AddDays(-60));

        var first = _world.Play(_knights, guard, EsportsWorld.Now.AddDays(-3), 2, 0);
        first.AddPlayerStats(link.Id, _knights.Id, 10, 2, 4, 3000, 1.40m);
        first.AddPlayerStats(impa.Id, guard.Id, 3, 9, 1, 1500, 0.70m);

        var second = _world.Play(guard, _knights, EsportsWorld.Now.AddDays(-2), 2, 1);
        second.AddPlayerStats(link.Id, _knights.Id, 6, 4, 2, 2000, 1.00m);
    }

    [Fact]
    public async Task Without_the_feature_the_scenario_refuses()
    {
        var result = await Handle(viewer: Guid.CreateVersion7());

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EsportsErrors.FeatureRequired);
    }

    [Fact]
    public async Task Anonymous_viewer_is_refused() =>
        (await Handle(viewer: null)).IsFailure.ShouldBeTrue();

    [Fact]
    public async Task Player_averages_cover_only_matches_for_this_team()
    {
        var result = await Handle(Subscriber);

        var link = result.Value.Players.ShouldHaveSingleItem();
        link.Matches.ShouldBe(2);
        link.Kills.ShouldBe(8);
        link.Deaths.ShouldBe(3);
        link.Kda.ShouldBe((8 + 3) / 3d);
        link.Rating.ShouldBe(1.2, tolerance: 0.001);
    }

    [Fact]
    public async Task Tournament_breakdown_counts_played_and_won()
    {
        var result = await Handle(Subscriber);

        var record = result.Value.Tournaments.ShouldHaveSingleItem();
        record.Played.ShouldBe(2);
        record.Wins.ShouldBe(1);
        record.WinRate.ShouldBe(50);
    }

    private Task<Result<TeamAdvancedStatsDto>> Handle(Guid? viewer)
    {
        var handler = new GetTeamAdvancedStatsQueryHandler(
            _world.Read(_world.Stats),
            _world.Read(_world.Players),
            _world.Read(_world.Matches),
            _world.Read([_world.Tournament]),
            _entitlements,
            new StubCurrentUserService { UserId = viewer },
            new InMemoryQueryExecutor());

        return handler.Handle(new GetTeamAdvancedStatsQuery(_knights.Id), CancellationToken.None);
    }
}