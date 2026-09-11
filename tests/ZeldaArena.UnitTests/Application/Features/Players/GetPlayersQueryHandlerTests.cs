using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Players.Queries.GetPlayers;
using ZeldaArena.Domain.Enums;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Players;

public class GetPlayersQueryHandlerTests
{
    private readonly EsportsWorld _world = new();

    public GetPlayersQueryHandlerTests()
    {
        var knights = _world.AddTeam("Hyrule Knights", "HYR");
        var guard = _world.AddTeam("Kakariko Guard", "KAK");
        var pending = _world.AddUserTeam(Guid.CreateVersion7(), "Pending Squad", "PND");

        _world.Sign(knights, "link", EsportsWorld.Now.AddDays(-60), PlayerRole.Attacker);
        _world.Sign(guard, "impa", EsportsWorld.Now.AddDays(-60), PlayerRole.Support);
        _world.Sign(pending, "hidden", EsportsWorld.Now.AddDays(-5), PlayerRole.Flex);

        // Ушёл из команды — теперь свободный игрок.
        var former = _world.Sign(knights, "rauru", EsportsWorld.Now.AddDays(-60), PlayerRole.Defender);
        knights.RemovePlayer(former.Id, EsportsWorld.Now.AddDays(-1));

        _world.AddPlayer("zelda", PlayerRole.Coach, country: "JP");
    }

    [Fact]
    public async Task Players_are_listed_by_nickname_with_their_current_team()
    {
        var result = await Handle(new GetPlayersQuery());

        result.Items.Select(player => player.Nickname).ShouldBe(["hidden", "impa", "link", "rauru", "zelda"]);
        result.Items.Single(player => player.Nickname == "link").TeamSlug.ShouldBe("hyrule-knights");
    }

    [Fact]
    public async Task Player_who_left_is_a_free_agent()
    {
        var result = await Handle(new GetPlayersQuery { Search = "rauru" });

        result.Items.ShouldHaveSingleItem().TeamName.ShouldBeNull();
    }

    /// <summary>Команда на модерации публично не существует (ADR-0008) — её игрок выглядит свободным.</summary>
    [Fact]
    public async Task Team_awaiting_moderation_is_not_shown_as_a_current_team()
    {
        var result = await Handle(new GetPlayersQuery { Search = "hidden" });

        var player = result.Items.ShouldHaveSingleItem();
        player.TeamName.ShouldBeNull();
        player.TeamId.ShouldBeNull();
    }

    [Fact]
    public async Task Team_filter_uses_the_current_roster_only()
    {
        var result = await Handle(new GetPlayersQuery { Team = "hyrule-knights" });

        result.Items.ShouldHaveSingleItem().Nickname.ShouldBe("link");
    }

    [Fact]
    public async Task Role_and_country_filters_narrow_the_list()
    {
        (await Handle(new GetPlayersQuery { Role = PlayerRole.Support })).Items.ShouldHaveSingleItem().Nickname.ShouldBe("impa");
        (await Handle(new GetPlayersQuery { Country = "jp" })).Items.ShouldHaveSingleItem().Nickname.ShouldBe("zelda");
    }

    [Theory]
    [InlineData("XX1")]
    [InlineData("no-such-team")]
    public async Task Unknown_filter_value_gives_an_empty_list_instead_of_being_dropped(string value)
    {
        var byCountry = await Handle(new GetPlayersQuery { Country = value });
        var byTeam = await Handle(new GetPlayersQuery { Team = value });

        byCountry.TotalCount.ShouldBe(0);
        byTeam.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Descending_nickname_sort_is_available()
    {
        var result = await Handle(new GetPlayersQuery { Sort = PlayerSorting.NicknameDescending });

        result.Items.First().Nickname.ShouldBe("zelda");
    }

    private Task<PagedResult<PlayerListItemDto>> Handle(GetPlayersQuery query) =>
        new GetPlayersQueryHandler(
                _world.Read(_world.Players),
                _world.Read(_world.RosterEntries),
                _world.Read(_world.Teams),
                new InMemoryQueryExecutor())
            .Handle(query, CancellationToken.None);
}