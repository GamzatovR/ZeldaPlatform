using ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Teams;

public class GetTeamBySlugQueryHandlerTests
{
    private static readonly Guid Owner = Guid.CreateVersion7();

    private readonly EsportsWorld _world = new();
    private readonly Team _knights;

    public GetTeamBySlugQueryHandlerTests()
    {
        _knights = _world.AddTeam("Hyrule Knights", "HYR");
        var guard = _world.AddTeam("Kakariko Guard", "KAK");
        _world.AddUserTeam(Owner, "Pending Squad", "PND");

        _world.Sign(_knights, "link", EsportsWorld.Now.AddDays(-60));

        // От старых к свежим: победа, поражение, победа — форма от свежего: W L W.
        _world.Play(_knights, guard, EsportsWorld.Now.AddDays(-3), 2, 0);
        _world.Play(guard, _knights, EsportsWorld.Now.AddDays(-2), 2, 1);
        _world.Play(guard, _knights, EsportsWorld.Now.AddDays(-1), 0, 2);
        _world.Schedule(_knights, guard, EsportsWorld.Now.AddDays(2));
    }

    [Fact]
    public async Task Basic_stats_count_played_and_won_matches()
    {
        var result = await Handle("hyrule-knights");

        result.ShouldNotBeNull();
        result.MatchesPlayed.ShouldBe(3);
        result.Wins.ShouldBe(2);
        result.Losses.ShouldBe(1);
        result.WinRate.ShouldBe(67);
    }

    [Fact]
    public async Task Form_goes_from_the_latest_match()
    {
        var result = await Handle("hyrule-knights");

        result.ShouldNotBeNull();
        result.Form.ShouldBe([true, false, true]);
    }

    [Fact]
    public async Task Roster_and_upcoming_matches_are_included()
    {
        var result = await Handle("hyrule-knights");

        result.ShouldNotBeNull();
        result.Roster.ShouldHaveSingleItem().Nickname.ShouldBe("link");
        result.UpcomingMatches.ShouldHaveSingleItem();
    }

    /// <summary>Команда на модерации для посторонних не существует — тот же ответ, что на чужой адрес.</summary>
    [Fact]
    public async Task Team_awaiting_moderation_is_hidden_from_other_viewers()
    {
        (await Handle("pending-squad", viewer: Guid.CreateVersion7())).ShouldBeNull();
        (await Handle("pending-squad", viewer: null)).ShouldBeNull();
    }

    [Fact]
    public async Task Owner_sees_the_team_awaiting_moderation()
    {
        var result = await Handle("pending-squad", viewer: Owner);

        result.ShouldNotBeNull();
        result.IsApproved.ShouldBeFalse();
        result.IsOwnedByViewer.ShouldBeTrue();
    }

    [Fact]
    public async Task Unknown_slug_is_not_found() =>
        (await Handle("no-such-team")).ShouldBeNull();

    private Task<TeamDetailsDto?> Handle(string slug, Guid? viewer = null)
    {
        var handler = new GetTeamBySlugQueryHandler(
            _world.Read(_world.Teams),
            _world.Read(_world.RosterEntries),
            _world.Read(_world.Players),
            _world.Read(_world.Matches),
            new StubCurrentUserService { UserId = viewer },
            new InMemoryQueryExecutor());

        return handler.Handle(new GetTeamBySlugQuery(slug), CancellationToken.None);
    }
}