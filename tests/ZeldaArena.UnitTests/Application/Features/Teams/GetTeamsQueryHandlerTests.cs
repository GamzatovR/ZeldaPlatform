using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;
using ZeldaArena.Domain.Enums;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Teams;

/// <summary>Список команд тем же механизмом, что турниры (docs/SPEC.md §10.2).</summary>
public class GetTeamsQueryHandlerTests
{
    private readonly EsportsWorld _world = new();

    public GetTeamsQueryHandlerTests()
    {
        var knights = _world.AddTeam("Hyrule Knights", "HYR", Region.Europe, rating: 1800);
        _world.AddTeam("Kakariko Guard", "KAK", Region.Europe, rating: 1500);
        _world.AddTeam("Zora Domain", "ZOR", Region.Asia, rating: 1650);
        _world.AddUserTeam(Guid.CreateVersion7(), "Pending Squad", "PND");

        _world.Sign(knights, "link", EsportsWorld.Now.AddDays(-30));
        var gone = _world.Sign(knights, "rauru", EsportsWorld.Now.AddDays(-30));
        knights.RemovePlayer(gone.Id, EsportsWorld.Now.AddDays(-1));
    }

    [Fact]
    public async Task Default_order_is_by_rating_descending()
    {
        var result = await Handle(new GetTeamsQuery());

        result.Items.Select(team => team.Name).ShouldBe(["Hyrule Knights", "Zora Domain", "Kakariko Guard"]);
    }

    /// <summary>Команда подписчика до одобрения в списке не показывается (ADR-0008).</summary>
    [Fact]
    public async Task Teams_awaiting_moderation_are_not_listed()
    {
        var result = await Handle(new GetTeamsQuery());

        result.Items.ShouldNotContain(team => team.Name == "Pending Squad");
    }

    [Fact]
    public async Task Region_and_rating_filters_combine()
    {
        var result = await Handle(new GetTeamsQuery { Region = Region.Europe, RatingMin = 1600 });

        result.Items.ShouldHaveSingleItem().Name.ShouldBe("Hyrule Knights");
    }

    [Theory]
    [InlineData("zora")]
    [InlineData("ZOR")]
    public async Task Search_matches_name_or_tag(string search)
    {
        var result = await Handle(new GetTeamsQuery { Search = search });

        result.Items.ShouldHaveSingleItem().Name.ShouldBe("Zora Domain");
    }

    /// <summary>Ушедший игрок — история, а не состав: считаются только открытые записи.</summary>
    [Fact]
    public async Task Player_count_includes_only_the_current_roster()
    {
        var result = await Handle(new GetTeamsQuery { Search = "Hyrule" });

        result.Items.ShouldHaveSingleItem().PlayerCount.ShouldBe(1);
    }

    [Fact]
    public async Task Name_sort_is_available()
    {
        var result = await Handle(new GetTeamsQuery { Sort = TeamSorting.NameAscending });

        result.Items.First().Name.ShouldBe("Hyrule Knights");
        result.Items.Last().Name.ShouldBe("Zora Domain");
    }

    [Fact]
    public void Validator_rejects_a_negative_rating() =>
        new GetTeamsQueryValidator().Validate(new GetTeamsQuery { RatingMin = -1 }).IsValid.ShouldBeFalse();

    private Task<PagedResult<TeamListItemDto>> Handle(GetTeamsQuery query) =>
        new GetTeamsQueryHandler(_world.Read(_world.Teams), new InMemoryQueryExecutor())
            .Handle(query, CancellationToken.None);
}