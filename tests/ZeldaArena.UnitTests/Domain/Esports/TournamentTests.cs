using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.Esports;

public class TournamentTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Team_is_added_once_with_its_seed()
    {
        var tournament = Announce();
        var team = Guid.CreateVersion7();

        tournament.AddTeam(team, 3);

        tournament.Participants.ShouldHaveSingleItem().Seed.ShouldBe(3);
        Should.Throw<InvariantViolationException>(() => tournament.AddTeam(team, 4))
            .Code.ShouldBe("tournament.team_already_participates");
    }

    [Fact]
    public void Seed_and_placement_change_through_the_tournament()
    {
        var tournament = Announce();
        var team = Guid.CreateVersion7();
        tournament.AddTeam(team, 1);

        tournament.ChangeSeed(team, 5);
        tournament.SetPlacement(team, 2);

        var participant = tournament.Participants.ShouldHaveSingleItem();
        participant.Seed.ShouldBe(5);
        participant.Placement.ShouldBe(2);
    }

    [Fact]
    public void Seed_of_a_team_outside_the_tournament_is_rejected() =>
        Should.Throw<InvariantViolationException>(() => Announce().ChangeSeed(Guid.CreateVersion7(), 1))
            .Code.ShouldBe("tournament.team_not_found");

    [Theory]
    [InlineData(TournamentStatus.Finished)]
    [InlineData(TournamentStatus.Canceled)]
    public void Roster_of_a_closed_tournament_cannot_lose_a_team(TournamentStatus status)
    {
        var tournament = Announce();
        var team = Guid.CreateVersion7();
        tournament.AddTeam(team, 1);
        tournament.Start();

        if (status == TournamentStatus.Finished)
        {
            tournament.Finish();
        }
        else
        {
            tournament.Cancel();
        }

        Should.Throw<InvariantViolationException>(() => tournament.RemoveTeam(team))
            .Code.ShouldBe("tournament.roster_is_closed");
        tournament.Participants.ShouldHaveSingleItem();
    }

    [Fact]
    public void Team_leaves_an_open_roster()
    {
        var tournament = Announce();
        var team = Guid.CreateVersion7();
        tournament.AddTeam(team, 1);
        tournament.Start();

        tournament.RemoveTeam(team);

        tournament.Participants.ShouldBeEmpty();
    }

    [Fact]
    public void Tournament_cannot_finish_before_it_starts() =>
        Should.Throw<InvariantViolationException>(() => Announce().Finish())
            .Code.ShouldBe("tournament.cannot_finish");

    private static Tournament Announce() =>
        Tournament.Announce(
            Slug.From("hyrule-open"),
            "Hyrule Open",
            TournamentTier.A,
            Region.Europe,
            Money.FromRubles(100_000m),
            Now,
            Now.AddDays(3));
}