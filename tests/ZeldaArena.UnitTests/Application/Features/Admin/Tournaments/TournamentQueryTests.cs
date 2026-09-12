using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;
using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Admin.Tournaments;

public class TournamentQueryTests
{
    private static readonly DateTimeOffset Now = EsportsWorld.Now;

    private readonly EsportsWorld _world = new();

    [Fact]
    public async Task Edit_card_lists_participants_with_their_matches_and_offers_only_free_approved_teams()
    {
        var knights = _world.AddTeam("Hyrule Knights", "HYR");
        var gerudo = _world.AddTeam("Gerudo Valley", "GRD");
        var free = _world.AddTeam("Lost Woods", "LWD");
        _world.AddUserTeam(Guid.CreateVersion7(), "Kokiri", "KOK");
        _world.Tournament.AddTeam(knights.Id, 2);
        _world.Tournament.AddTeam(gerudo.Id, 1);
        _world.Schedule(knights, gerudo, Now.AddDays(1));

        var card = await new GetTournamentForEditQueryHandler(
                _world.Read([_world.Tournament]), _world.Read(_world.Teams), _world.Read(_world.Matches), new InMemoryQueryExecutor())
            .Handle(new GetTournamentForEditQuery(_world.Tournament.Id), CancellationToken.None);

        card.ShouldNotBeNull();
        card.Participants.Select(participant => participant.TeamName).ShouldBe(["Gerudo Valley", "Hyrule Knights"], "по посеву");
        card.Participants.ShouldAllBe(participant => participant.MatchCount == 1);
        card.AvailableTeams.ShouldHaveSingleItem().Id.ShouldBe(free.Id);
        card.MatchCount.ShouldBe(1);
        card.NextSeed.ShouldBe(3);
    }

    [Fact]
    public async Task Missing_tournament_gives_no_card()
    {
        var card = await new GetTournamentForEditQueryHandler(
                _world.Read([_world.Tournament]), _world.Read(_world.Teams), _world.Read(_world.Matches), new InMemoryQueryExecutor())
            .Handle(new GetTournamentForEditQuery(Guid.CreateVersion7()), CancellationToken.None);

        card.ShouldBeNull();
    }

    [Fact]
    public async Task Table_shows_every_status_filters_by_name_and_sorts_by_the_whitelisted_key()
    {
        var canceled = Tournament.Announce(
            Slug.From("zora-cup"), "Zora Cup", TournamentTier.C, Region.Asia, Money.FromRubles(1m), Now, Now.AddDays(1));
        canceled.Cancel();
        var another = Tournament.Announce(
            Slug.From("zora-league"), "Zora League", TournamentTier.B, Region.Asia, Money.FromRubles(5m), Now, Now.AddDays(1));
        var handler = new GetTournamentsForAdminQueryHandler(
            _world.Read([_world.Tournament, canceled, another]), new InMemoryQueryExecutor());

        var page = await handler.Handle(
            new GetTournamentsForAdminQuery { Search = "zora", Sort = AdminTournamentSorting.NameDescending },
            CancellationToken.None);

        page.Items.Select(item => item.Name).ShouldBe(["Zora League", "Zora Cup"]);
        page.Items.ShouldContain(item => item.Status == TournamentStatus.Canceled, "админка видит и отменённые");
    }
}