using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.Esports;

public class TeamTests
{
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Team_created_by_admin_is_approved_and_has_no_owner()
    {
        var team = CreateTeam();

        team.IsApproved.ShouldBeTrue();
        team.OwnerUserId.ShouldBeNull();
    }

    [Fact]
    public void Team_created_by_subscriber_waits_for_moderation()
    {
        var ownerId = Guid.CreateVersion7();

        var team = Team.CreateByUser(
            ownerId,
            Slug.From("hyrule-knights"),
            "Hyrule Knights",
            "HYR",
            CountryCode.From("RU"),
            Region.Cis);

        team.OwnerUserId.ShouldBe(ownerId);
        team.IsApproved.ShouldBeFalse();

        team.Approve();

        team.IsApproved.ShouldBeTrue();
    }

    [Fact]
    public void Team_created_by_user_requires_owner()
    {
        Should.Throw<InvariantViolationException>(() => Team.CreateByUser(
                Guid.Empty,
                Slug.From("hyrule-knights"),
                "Hyrule Knights",
                "HYR",
                CountryCode.From("RU"),
                Region.Cis))
            .Code.ShouldBe("team.owner_required");
    }

    [Fact]
    public void Tag_is_upper_cased()
    {
        CreateTeam(tag: "hyr").Tag.ShouldBe("HYR");
    }

    [Theory]
    [InlineData("H")]
    [InlineData("TOOLONGTAG")]
    public void Tag_length_is_checked(string tag)
    {
        Should.Throw<InvariantViolationException>(() => CreateTeam(tag: tag))
            .Code.ShouldBe("team.invalid_tag");
    }

    [Fact]
    public void Player_is_added_to_active_roster()
    {
        var team = CreateTeam();
        var playerId = Guid.CreateVersion7();

        var entry = team.AddPlayer(playerId, PlayerRole.Attacker, Now);

        entry.IsActive.ShouldBeTrue();
        entry.TeamId.ShouldBe(team.Id);
        team.ActiveRoster.ShouldHaveSingleItem();
    }

    [Fact]
    public void Player_cannot_be_added_to_the_same_team_twice()
    {
        var team = CreateTeam();
        var playerId = Guid.CreateVersion7();
        team.AddPlayer(playerId, PlayerRole.Attacker, Now);

        Should.Throw<InvariantViolationException>(
                () => team.AddPlayer(playerId, PlayerRole.Support, Now))
            .Code.ShouldBe("team.player_already_in_roster");
    }

    [Fact]
    public void Leaving_the_team_closes_the_entry_but_keeps_history()
    {
        var team = CreateTeam();
        var playerId = Guid.CreateVersion7();
        team.AddPlayer(playerId, PlayerRole.Attacker, Now);

        team.RemovePlayer(playerId, Now.AddMonths(6));

        team.RosterEntries.ShouldHaveSingleItem();
        team.ActiveRoster.ShouldBeEmpty();
        team.RosterEntries.Single().LeftAt.ShouldBe(Now.AddMonths(6));
    }

    [Fact]
    public void Player_can_return_to_the_team_as_a_new_entry()
    {
        var team = CreateTeam();
        var playerId = Guid.CreateVersion7();
        team.AddPlayer(playerId, PlayerRole.Attacker, Now);
        team.RemovePlayer(playerId, Now.AddMonths(6));

        team.AddPlayer(playerId, PlayerRole.Coach, Now.AddYears(1));

        team.RosterEntries.Count.ShouldBe(2);
        team.ActiveRoster.ShouldHaveSingleItem().Role.ShouldBe(PlayerRole.Coach);
    }

    [Fact]
    public void Leaving_date_cannot_precede_joining_date()
    {
        var team = CreateTeam();
        var playerId = Guid.CreateVersion7();
        team.AddPlayer(playerId, PlayerRole.Attacker, Now);

        Should.Throw<InvariantViolationException>(
                () => team.RemovePlayer(playerId, Now.AddDays(-1)))
            .Code.ShouldBe("roster.left_before_joined");
    }

    [Fact]
    public void Unknown_player_cannot_be_removed()
    {
        var team = CreateTeam();

        Should.Throw<InvariantViolationException>(
                () => team.RemovePlayer(Guid.CreateVersion7(), Now))
            .Code.ShouldBe("team.player_not_in_roster");
    }

    [Fact]
    public void Rating_cannot_be_negative()
    {
        Should.Throw<InvariantViolationException>(() => CreateTeam().UpdateRating(-1))
            .Code.ShouldBe("team.negative_rating");
    }

    private static Team CreateTeam(string tag = "HYR") =>
        Team.Create(
            Slug.From("hyrule-knights"),
            "Hyrule Knights",
            tag,
            CountryCode.From("RU"),
            Region.Cis,
            rating: 1500);
}