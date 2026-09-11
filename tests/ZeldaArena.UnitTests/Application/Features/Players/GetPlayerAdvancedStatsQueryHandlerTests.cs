using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Players;

public class GetPlayerAdvancedStatsQueryHandlerTests
{
    private static readonly Guid Subscriber = Guid.CreateVersion7();

    private readonly EsportsWorld _world = new();
    private readonly Player _link;

    public GetPlayerAdvancedStatsQueryHandlerTests()
    {
        var knights = _world.AddTeam("Hyrule Knights", "HYR");
        var guard = _world.AddTeam("Kakariko Guard", "KAK");
        _link = _world.Sign(knights, "link", EsportsWorld.Now.AddDays(-60));

        _world.Play(knights, guard, EsportsWorld.Now.AddDays(-3), 2, 0)
            .AddPlayerStats(_link.Id, knights.Id, 10, 2, 4, 3000, 1.40m);
        _world.Play(knights, guard, EsportsWorld.Now.AddDays(-1), 2, 1)
            .AddPlayerStats(_link.Id, knights.Id, 6, 4, 2, 2000, 1.00m);
    }

    [Fact]
    public async Task Without_the_feature_the_scenario_refuses()
    {
        var result = await Handle(Guid.CreateVersion7());

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EsportsErrors.FeatureRequired);
    }

    [Fact]
    public async Task Subscriber_gets_averages_per_tournament()
    {
        var result = await Handle(Subscriber);

        var tournament = result.Value.ShouldHaveSingleItem();
        tournament.Name.ShouldBe("Hyrule Open");
        tournament.Matches.ShouldBe(2);
        tournament.Damage.ShouldBe(2500);
    }

    private Task<Result<IReadOnlyList<PlayerTournamentStatsDto>>> Handle(Guid viewer) =>
        new GetPlayerAdvancedStatsQueryHandler(
                _world.Read(_world.Stats),
                _world.Read(_world.Matches),
                _world.Read([_world.Tournament]),
                new StubEntitlementService().Grant(Subscriber, FeatureCodes.StatsAdvanced),
                new StubCurrentUserService { UserId = viewer },
                new InMemoryQueryExecutor())
            .Handle(new GetPlayerAdvancedStatsQuery(_link.Id), CancellationToken.None);
}