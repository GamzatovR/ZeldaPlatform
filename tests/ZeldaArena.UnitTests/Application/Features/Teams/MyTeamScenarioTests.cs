using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Teams.Commands.AddFreeAgentToMyTeam;
using ZeldaArena.Application.Features.Teams.Commands.AddNewPlayerToMyTeam;
using ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamLogo;
using ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamPlayerRole;
using ZeldaArena.Application.Features.Teams.Commands.RemovePlayerFromMyTeam;
using ZeldaArena.Application.Features.Teams.Commands.UpdateMyTeamProfile;
using ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;
using ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Teams;

/// <summary>
/// Кабинет капитана (docs/SPEC.md §9.3, п. 8). Три правила, которые нельзя доверить
/// одному атрибуту на действии:
/// <list type="bullet">
///   <item>чужую команду не правит никто (IDOR, §15);</item>
///   <item>после истечения подписки команда видна, но не правится (docs/CONVENTIONS.md);</item>
///   <item>игрок не состоит в двух командах одновременно (§15).</item>
/// </list>
/// </summary>
public class MyTeamScenarioTests
{
    private static readonly Guid Captain = Guid.CreateVersion7();
    private static readonly Guid Stranger = Guid.CreateVersion7();

    private readonly EsportsWorld _world = new();
    private readonly StubEntitlementService _entitlements = new StubEntitlementService()
        .Grant(Captain, FeatureCodes.TeamCreate)
        .Grant(Stranger, FeatureCodes.TeamCreate);

    private readonly InMemoryFileStorage _storage = new();
    private readonly RecordingUnitOfWork _unitOfWork = new();
    private readonly FixedDateTimeProvider _clock = new(EsportsWorld.Now);

    private readonly Team _mine;
    private readonly Team _knights;
    private readonly Player _link;
    private readonly Player _freeAgent;

    public MyTeamScenarioTests()
    {
        _mine = _world.AddUserTeam(Captain, "Guardians", "GRD");
        _knights = _world.AddTeam("Hyrule Knights", "HYR");

        _link = _world.Sign(_knights, "link", EsportsWorld.Now.AddDays(-30));
        _freeAgent = _world.AddPlayer("rauru", PlayerRole.Support);
        _world.Sign(_mine, "impa", EsportsWorld.Now.AddDays(-3));
    }

    [Fact]
    public async Task Captain_adds_a_free_agent()
    {
        var result = await AddFreeAgent(_freeAgent.Id);

        result.IsSuccess.ShouldBeTrue();
        _mine.ActiveRoster.ShouldContain(entry => entry.PlayerId == _freeAgent.Id && entry.Role == PlayerRole.Flex);
        _unitOfWork.SaveChangesCalls.ShouldBe(1);
    }

    /// <summary>Переманить игрока из чужой команды нельзя (§15, ADR-0008).</summary>
    [Fact]
    public async Task Player_of_another_team_cannot_be_taken()
    {
        var result = await AddFreeAgent(_link.Id);

        result.Error.ShouldBe(EsportsErrors.PlayerInAnotherTeam);
        _mine.ActiveRoster.ShouldNotContain(entry => entry.PlayerId == _link.Id);
    }

    [Fact]
    public async Task Unknown_player_is_not_found() =>
        (await AddFreeAgent(Guid.CreateVersion7())).Error.ShouldBe(EsportsErrors.PlayerNotFound);

    /// <summary>Чужая команда — тот же ответ, что несуществующая: перебирать идентификаторы бесполезно.</summary>
    [Fact]
    public async Task Another_owners_team_cannot_be_edited()
    {
        var result = await AddFreeAgent(_freeAgent.Id, user: Stranger);

        result.Error.ShouldBe(EsportsErrors.TeamNotFound);
        _mine.ActiveRoster.ShouldNotContain(entry => entry.PlayerId == _freeAgent.Id);
    }

    [Fact]
    public async Task Admin_seeded_team_has_no_owner_and_cannot_be_edited_by_anyone() =>
        (await Send(new RemovePlayerFromMyTeamCommand(_knights.Id, _link.Id))).Error.ShouldBe(EsportsErrors.TeamNotFound);

    /// <summary>Подписка истекла — команда остаётся, но правка закрыта с любого входа.</summary>
    [Fact]
    public async Task Without_the_feature_editing_is_refused()
    {
        _entitlements.Revoke(Captain, FeatureCodes.TeamCreate);

        var result = await AddFreeAgent(_freeAgent.Id);

        result.Error.ShouldBe(EsportsErrors.FeatureRequired);
    }

    [Fact]
    public async Task New_player_profile_is_created_straight_into_the_roster()
    {
        var players = new InMemoryRepository<Player>();

        var result = await new AddNewPlayerToMyTeamCommandHandler(
                Teams(),
                players,
                _world.Read(_world.Players),
                new InMemoryQueryExecutor(),
                _entitlements,
                new StubCurrentUserService { UserId = Captain },
                _clock,
                _unitOfWork)
            .Handle(
                new AddNewPlayerToMyTeamCommand
                {
                    TeamId = _mine.Id,
                    Nickname = "Ганондорф",
                    Role = PlayerRole.Defender,
                    CountryCode = "RU",
                },
                CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var player = players.Entities.ShouldHaveSingleItem();
        player.Slug.Value.ShouldBe("ganondorf");
        _mine.ActiveRoster.ShouldContain(entry => entry.PlayerId == player.Id);
    }

    /// <summary>Состав историчен: уход закрывает запись датой, а не удаляет её (docs/CONVENTIONS.md).</summary>
    [Fact]
    public async Task Removing_a_player_closes_the_roster_entry()
    {
        var impa = _world.Players.Single(player => player.Nickname == "impa");

        var result = await Send(new RemovePlayerFromMyTeamCommand(_mine.Id, impa.Id));

        result.IsSuccess.ShouldBeTrue();
        var entry = _mine.RosterEntries.ShouldHaveSingleItem();
        entry.LeftAt.ShouldBe(EsportsWorld.Now);
    }

    [Fact]
    public async Task Removing_someone_not_in_the_roster_is_a_clear_refusal() =>
        (await Send(new RemovePlayerFromMyTeamCommand(_mine.Id, _link.Id))).Error.ShouldBe(EsportsErrors.PlayerNotFound);

    [Fact]
    public async Task Role_is_changed_in_place()
    {
        var impa = _world.Players.Single(player => player.Nickname == "impa");

        var result = await Send(new ChangeMyTeamPlayerRoleCommand(_mine.Id, impa.Id, PlayerRole.Coach));

        result.IsSuccess.ShouldBeTrue();
        _mine.ActiveRoster.Single().Role.ShouldBe(PlayerRole.Coach);
    }

    /// <summary>Слаг — адрес, а не подпись: ссылки на страницу команды переживают переименование.</summary>
    [Fact]
    public async Task Profile_rename_keeps_the_slug()
    {
        var result = await UpdateProfile("Стражи Хайрула");

        result.IsSuccess.ShouldBeTrue();
        _mine.Name.ShouldBe("Стражи Хайрула");
        _mine.Slug.Value.ShouldBe("guardians");
    }

    [Fact]
    public async Task Profile_cannot_take_another_teams_name() =>
        (await UpdateProfile("hyrule KNIGHTS")).Error.ShouldBe(EsportsErrors.TeamNameTaken);

    [Fact]
    public async Task Profile_can_keep_its_own_name() =>
        (await UpdateProfile("GUARDIANS")).IsSuccess.ShouldBeTrue();

    [Fact]
    public async Task New_logo_replaces_the_old_file()
    {
        var first = await ChangeLogo(SampleImages.Png);
        var oldLogo = _mine.LogoPath;

        var second = await ChangeLogo(SampleImages.Webp);

        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        _mine.LogoPath.ShouldNotBe(oldLogo);
        _storage.Deleted.ShouldBe([oldLogo!]);
        _storage.Files.Keys.ShouldBe([_mine.LogoPath!]);
    }

    [Fact]
    public async Task Logo_that_is_not_an_image_is_refused() =>
        (await ChangeLogo(SampleImages.Executable)).Error.ShouldBe(FileErrors.InvalidImage);

    [Fact]
    public async Task Captain_page_is_read_only_without_the_feature()
    {
        _entitlements.Revoke(Captain, FeatureCodes.TeamCreate);

        var page = await MyTeam(Captain);

        page.ShouldNotBeNull();
        page.CanEdit.ShouldBeFalse();
        page.Roster.ShouldHaveSingleItem().Nickname.ShouldBe("impa");
    }

    [Fact]
    public async Task Someone_elses_team_id_does_not_open_their_page() =>
        (await MyTeam(Stranger, _mine.Id)).ShouldBeNull();

    [Fact]
    public async Task Free_agents_are_players_without_an_open_roster_entry()
    {
        var agents = await new GetFreeAgentsQueryHandler(
                _world.Read(_world.Players),
                _world.Read(_world.RosterEntries),
                new InMemoryQueryExecutor())
            .Handle(new GetFreeAgentsQuery(), CancellationToken.None);

        agents.Select(agent => agent.Nickname).ShouldBe(["rauru"]);
    }

    private InMemoryRepository<Team> Teams() => new([.. _world.Teams]);

    private StubCurrentUserService User(Guid? id) => new() { UserId = id };

    private Task<Result> AddFreeAgent(Guid playerId, Guid? user = null) =>
        new AddFreeAgentToMyTeamCommandHandler(
                Teams(),
                _world.Read(_world.Players),
                _world.Read(_world.RosterEntries),
                new InMemoryQueryExecutor(),
                _entitlements,
                User(user ?? Captain),
                _clock,
                _unitOfWork)
            .Handle(new AddFreeAgentToMyTeamCommand(_mine.Id, playerId, PlayerRole.Flex), CancellationToken.None);

    private Task<Result> Send(RemovePlayerFromMyTeamCommand command) =>
        new RemovePlayerFromMyTeamCommandHandler(Teams(), _entitlements, User(Captain), _clock, _unitOfWork)
            .Handle(command, CancellationToken.None);

    private Task<Result> Send(ChangeMyTeamPlayerRoleCommand command) =>
        new ChangeMyTeamPlayerRoleCommandHandler(Teams(), _entitlements, User(Captain), _unitOfWork)
            .Handle(command, CancellationToken.None);

    private Task<Result> UpdateProfile(string name) =>
        new UpdateMyTeamProfileCommandHandler(
                Teams(),
                _world.Read(_world.Teams),
                new InMemoryQueryExecutor(),
                _entitlements,
                User(Captain),
                _unitOfWork)
            .Handle(
                new UpdateMyTeamProfileCommand
                {
                    TeamId = _mine.Id,
                    Name = name,
                    Tag = "GRD",
                    CountryCode = "RU",
                    Region = Region.Cis,
                },
                CancellationToken.None);

    private Task<Result> ChangeLogo(byte[] content) =>
        new ChangeMyTeamLogoCommandHandler(Teams(), _entitlements, User(Captain), _storage, _unitOfWork)
            .Handle(
                new ChangeMyTeamLogoCommand(_mine.Id, new FileUpload(content.AsStream(), "logo.png", "image/png", content.Length)),
                CancellationToken.None);

    private Task<MyTeamDto?> MyTeam(Guid user, Guid? teamId = null) =>
        new GetMyTeamQueryHandler(
                _world.Read(_world.Teams),
                _world.Read(_world.RosterEntries),
                _world.Read(_world.Players),
                _entitlements,
                User(user),
                new InMemoryQueryExecutor())
            .Handle(new GetMyTeamQuery(teamId), CancellationToken.None);
}