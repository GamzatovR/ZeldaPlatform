using ZeldaArena.Application.Features.Teams.Queries.GetTopTeams;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Teams;

public class GetTopTeamsQueryHandlerTests
{
    [Fact]
    public async Task Best_rated_approved_teams_come_first()
    {
        var world = new EsportsWorld();
        world.AddTeam("Hyrule Knights", "HYR", rating: 1800);
        world.AddTeam("Kakariko Guard", "KAK", rating: 1500);
        world.AddTeam("Zora Domain", "ZOR", rating: 1650);
        world.AddUserTeam(Guid.CreateVersion7(), "Pending Squad", "PND");

        var result = await new GetTopTeamsQueryHandler(world.Read(world.Teams), new InMemoryQueryExecutor())
            .Handle(new GetTopTeamsQuery(Count: 2), CancellationToken.None);

        result.Select(team => team.Name).ShouldBe(["Hyrule Knights", "Zora Domain"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(GetTopTeamsQueryValidator.MaxCount + 1)]
    public void Validator_limits_the_count(int count) =>
        new GetTopTeamsQueryValidator().Validate(new GetTopTeamsQuery(count)).IsValid.ShouldBeFalse();
}