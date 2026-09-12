using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Admin.Teams.Commands.AddTeamPlayer;
using ZeldaArena.Application.Features.Admin.Teams.Commands.CreateTeamByAdmin;
using ZeldaArena.Application.Features.Admin.Teams.Commands.DeleteTeam;
using ZeldaArena.Application.Features.Admin.Teams.Commands.RemoveTeamPlayer;
using ZeldaArena.Application.Features.Admin.Teams.Commands.SetTeamApproval;
using ZeldaArena.Application.Features.Admin.Teams.Commands.UpdateTeamByAdmin;
using ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;
using ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Admin.Teams;

public class TeamAdminTests
{
    private static readonly DateTimeOffset Now = EsportsWorld.Now;

    private readonly EsportsWorld _world = new();
    private readonly RecordingUnitOfWork _unitOfWork = new();
    private readonly InMemoryFileStorage _storage = new();
    private readonly InMemoryRepository<Team> _teams = new();

    [Fact]
    public async Task Admin_created_team_is_approved_and_has_no_owner()
    {
        var result = await new CreateTeamByAdminCommandHandler(
                _teams, new InMemoryReadRepository<Team>(_teams.Entities), new InMemoryQueryExecutor(), _storage, _unitOfWork)
            .Handle(
                new CreateTeamByAdminCommand
                {
                    Name = "Стражи Хайрула",
                    Tag = "SHR",
                    CountryCode = "RU",
                    Region = Region.Cis,
                    Rating = 1200,
                },
                CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var team = _teams.Entities.ShouldHaveSingleItem();
        team.IsApproved.ShouldBeTrue("команду завела администрация, модерировать нечего");
        team.OwnerUserId.ShouldBeNull();
        team.Slug.Value.ShouldBe("strazhi-khayrula");
    }

    [Fact]
    public async Task Team_name_is_not_taken_twice()
    {
        var existing = _world.AddTeam("Hyrule Knights", "HYR");
        await _teams.AddAsync(existing, CancellationToken.None);

        var result = await new CreateTeamByAdminCommandHandler(
                _teams, new InMemoryReadRepository<Team>(_teams.Entities), new InMemoryQueryExecutor(), _storage, _unitOfWork)
            .Handle(
                new CreateTeamByAdminCommand { Name = "hyrule knights", Tag = "HYR2", CountryCode = "RU" },
                CancellationToken.None);

        result.Error.ShouldBe(EsportsErrors.TeamNameTaken);
    }

    [Fact]
    public async Task Renaming_a_team_to_its_own_name_is_allowed()
    {
        var team = _world.AddTeam("Hyrule Knights", "HYR");
        await _teams.AddAsync(team, CancellationToken.None);

        var result = await Update(team.Id, "Hyrule Knights", rating: 1500);

        result.IsSuccess.ShouldBeTrue();
        team.Rating.ShouldBe(1500);
    }

    [Fact]
    public async Task Approval_of_a_subscriber_team_opens_it_and_revoking_hides_it_again()
    {
        var team = _world.AddUserTeam(Guid.CreateVersion7(), "Kokiri", "KOK");
        await _teams.AddAsync(team, CancellationToken.None);
        var handler = new SetTeamApprovalCommandHandler(_teams, _unitOfWork);

        (await handler.Handle(new SetTeamApprovalCommand(team.Id, true), CancellationToken.None)).IsSuccess.ShouldBeTrue();
        team.IsApproved.ShouldBeTrue();

        (await handler.Handle(new SetTeamApprovalCommand(team.Id, false), CancellationToken.None)).IsSuccess.ShouldBeTrue();
        team.IsApproved.ShouldBeFalse();
        _unitOfWork.SaveChangesCalls.ShouldBe(2);
    }

    [Fact]
    public async Task Player_of_another_team_is_refused()
    {
        var first = _world.AddTeam("Hyrule Knights", "HYR");
        var second = _world.AddTeam("Gerudo Valley", "GRD");
        await _teams.AddAsync(first, CancellationToken.None);
        await _teams.AddAsync(second, CancellationToken.None);
        var player = _world.Sign(first, "Link", Now.AddYears(-1));

        var result = await AddPlayer(second.Id, player.Id);

        result.ShouldBe(EsportsErrors.PlayerInAnotherTeam);
    }

    [Fact]
    public async Task Free_agent_joins_and_leaving_only_closes_the_entry()
    {
        var team = _world.AddTeam("Hyrule Knights", "HYR");
        await _teams.AddAsync(team, CancellationToken.None);
        var player = _world.AddPlayer("Link");

        (await AddPlayer(team.Id, player.Id)).ShouldBe(Error.None);

        var removed = await new RemoveTeamPlayerCommandHandler(_teams, new FixedDateTimeProvider(Now), _unitOfWork)
            .Handle(new RemoveTeamPlayerCommand(team.Id, player.Id), CancellationToken.None);

        removed.IsSuccess.ShouldBeTrue();
        var entry = team.RosterEntries.ShouldHaveSingleItem();
        entry.LeftAt.ShouldBe(Now, "состав историчен: запись закрывается, а не удаляется");
        entry.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Team_with_roster_history_is_not_deleted()
    {
        var team = _world.AddTeam("Hyrule Knights", "HYR");
        await _teams.AddAsync(team, CancellationToken.None);
        _world.Sign(team, "Link", Now.AddYears(-1));

        var result = await Delete(team.Id);

        result.Error.ShouldBe(EsportsErrors.TeamHasHistory);
        _teams.Entities.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Team_that_played_is_not_deleted()
    {
        var teamA = _world.AddTeam("Hyrule Knights", "HYR");
        var teamB = _world.AddTeam("Gerudo Valley", "GRD");
        await _teams.AddAsync(teamA, CancellationToken.None);
        _world.Schedule(teamA, teamB, Now.AddDays(1));

        var result = await Delete(teamA.Id);

        result.Error.ShouldBe(EsportsErrors.TeamHasHistory);
    }

    [Fact]
    public async Task Team_in_a_tournament_is_not_deleted()
    {
        var team = _world.AddTeam("Hyrule Knights", "HYR");
        await _teams.AddAsync(team, CancellationToken.None);
        _world.Tournament.AddTeam(team.Id, 1);

        var result = await Delete(team.Id);

        result.Error.ShouldBe(EsportsErrors.TeamHasHistory);
    }

    [Fact]
    public async Task Team_without_history_is_deleted_with_its_logo()
    {
        var team = _world.AddTeam("Hyrule Knights", "HYR");
        team.ChangeLogo("0123456789abcdef0123456789abcdef.png");
        await _teams.AddAsync(team, CancellationToken.None);

        var result = await Delete(team.Id);

        result.IsSuccess.ShouldBeTrue();
        _teams.Entities.ShouldBeEmpty();
        _storage.Deleted.ShouldBe(["0123456789abcdef0123456789abcdef.png"]);
    }

    [Fact]
    public async Task Admin_table_shows_teams_awaiting_moderation()
    {
        _world.AddTeam("Hyrule Knights", "HYR");
        _world.AddUserTeam(Guid.CreateVersion7(), "Kokiri", "KOK");

        var page = await new GetTeamsForAdminQueryHandler(
                _world.Read(_world.Teams), _world.Read(_world.Matches), new InMemoryQueryExecutor())
            .Handle(new GetTeamsForAdminQuery { IsApproved = false }, CancellationToken.None);

        var row = page.Items.ShouldHaveSingleItem();
        row.Name.ShouldBe("Kokiri");
        row.IsUserOwned.ShouldBeTrue();
    }

    [Fact]
    public async Task Edit_card_counts_history_and_offers_only_free_players()
    {
        var team = _world.AddTeam("Hyrule Knights", "HYR");
        var rival = _world.AddTeam("Gerudo Valley", "GRD");
        _world.Sign(team, "Link", Now.AddYears(-1));
        var free = _world.AddPlayer("Zelda");
        _world.Schedule(team, rival, Now.AddDays(1));
        _world.Tournament.AddTeam(team.Id, 1);

        var card = await new GetTeamForEditQueryHandler(
                _world.Read(_world.Teams),
                _world.Read(_world.RosterEntries),
                _world.Read(_world.Players),
                _world.Read(_world.Matches),
                _world.Read([_world.Tournament]),
                new InMemoryQueryExecutor())
            .Handle(new GetTeamForEditQuery(team.Id), CancellationToken.None);

        card.ShouldNotBeNull();
        card.Roster.ShouldHaveSingleItem().Nickname.ShouldBe("Link");
        card.FreeAgents.Select(agent => agent.Id).ShouldContain(free.Id);
        card.FreeAgents.ShouldNotContain(agent => agent.Nickname == "Link");
        card.MatchCount.ShouldBe(1);
        card.TournamentCount.ShouldBe(1);
        card.CanDelete.ShouldBeFalse();
    }

    private Task<Result> Update(Guid id, string name, int rating) =>
        new UpdateTeamByAdminCommandHandler(
                _teams, new InMemoryReadRepository<Team>(_teams.Entities), new InMemoryQueryExecutor(), _storage, _unitOfWork)
            .Handle(
                new UpdateTeamByAdminCommand
                {
                    Id = id,
                    Name = name,
                    Tag = "HYR",
                    CountryCode = "RU",
                    Region = Region.Cis,
                    Rating = rating,
                },
                CancellationToken.None);

    private async Task<Error> AddPlayer(Guid teamId, Guid playerId)
    {
        var result = await new AddTeamPlayerCommandHandler(
                _teams,
                _world.Read(_world.Players),
                _world.Read(_world.RosterEntries),
                new InMemoryQueryExecutor(),
                new FixedDateTimeProvider(Now),
                _unitOfWork)
            .Handle(new AddTeamPlayerCommand(teamId, playerId, PlayerRole.Attacker), CancellationToken.None);

        return result.IsSuccess ? Error.None : result.Error;
    }

    private Task<Result> Delete(Guid id) =>
        new DeleteTeamCommandHandler(
                _teams,
                _world.Read(_world.Matches),
                _world.Read([_world.Tournament]),
                new InMemoryQueryExecutor(),
                _storage,
                _unitOfWork)
            .Handle(new DeleteTeamCommand(id), CancellationToken.None);
}