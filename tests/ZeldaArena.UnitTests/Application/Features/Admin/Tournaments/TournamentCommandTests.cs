using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.AddTournamentTeam;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.ChangeTournamentStatus;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.CreateTournament;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.DeleteTournament;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.RemoveTournamentTeam;
using ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournament;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Admin.Tournaments;

public class TournamentCommandTests
{
    private static readonly DateTimeOffset Now = EsportsWorld.Now;

    private readonly EsportsWorld _world = new();
    private readonly RecordingUnitOfWork _unitOfWork = new();
    private readonly InMemoryFileStorage _storage = new();
    private readonly StubHtmlSanitizer _sanitizer = new();
    private readonly InMemoryRepository<Tournament> _tournaments;

    public TournamentCommandTests() => _tournaments = new InMemoryRepository<Tournament>(_world.Tournament);

    [Fact]
    public async Task Created_tournament_gets_a_slug_sanitized_rules_and_a_stored_logo()
    {
        var result = await new CreateTournamentCommandHandler(
                _tournaments, new InMemoryReadRepository<Tournament>(_tournaments.Entities), new InMemoryQueryExecutor(),
                _sanitizer, _storage, _unitOfWork)
            .Handle(
                new CreateTournamentCommand
                {
                    Name = "Кубок Хайрула",
                    Tier = TournamentTier.S,
                    Region = Region.Cis,
                    PrizePool = 50_000m,
                    StartsAt = Now,
                    EndsAt = Now.AddDays(2),
                    RulesHtml = "<p>Bo3</p><script>alert(1)</script>",
                    IsFeatured = true,
                    Logo = new FileUpload(SampleImages.Png.AsStream(), "logo.png", "image/png", SampleImages.Png.Length),
                },
                CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var created = _tournaments.Entities.Single(item => item.Id == result.Value);
        created.Slug.Value.ShouldBe("kubok-khayrula");
        created.RulesHtml.ShouldBe(StubHtmlSanitizer.Mark + "<p>Bo3</p>", "регламент очищается до сохранения, а не при выводе");
        created.IsFeatured.ShouldBeTrue();
        created.LogoPath.ShouldNotBeNull();
        _storage.Files.ShouldContainKey(created.LogoPath);
        _unitOfWork.SaveChangesCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Slug_of_a_taken_name_gets_a_number()
    {
        var result = await new CreateTournamentCommandHandler(
                _tournaments, new InMemoryReadRepository<Tournament>(_tournaments.Entities), new InMemoryQueryExecutor(),
                _sanitizer, _storage, _unitOfWork)
            .Handle(Command("Hyrule Open"), CancellationToken.None);

        _tournaments.Entities.Single(item => item.Id == result.Value).Slug.Value.ShouldBe("hyrule-open-2");
    }

    [Fact]
    public async Task Executable_under_an_image_name_is_rejected_before_anything_is_stored()
    {
        var command = Command("Кубок") with
        {
            Logo = new FileUpload(SampleImages.Executable.AsStream(), "logo.png", "image/png", SampleImages.Executable.Length),
        };

        var result = await new CreateTournamentCommandHandler(
                _tournaments, new InMemoryReadRepository<Tournament>(_tournaments.Entities), new InMemoryQueryExecutor(),
                _sanitizer, _storage, _unitOfWork)
            .Handle(command, CancellationToken.None);

        result.Error.ShouldBe(FileErrors.InvalidImage);
        _storage.Files.ShouldBeEmpty();
        _unitOfWork.SaveChangesCalls.ShouldBe(0);
    }

    [Fact]
    public async Task Dates_in_the_wrong_order_are_refused_by_the_entity_not_by_a_crash()
    {
        var result = await new UpdateTournamentCommandHandler(_tournaments, _sanitizer, _storage, _unitOfWork)
            .Handle(
                new UpdateTournamentCommand
                {
                    Id = _world.Tournament.Id,
                    Name = "Hyrule Open",
                    StartsAt = Now,
                    EndsAt = Now.AddDays(-1),
                },
                CancellationToken.None);

        result.Error.Code.ShouldBe("tournament.ends_before_starts");
        _unitOfWork.SaveChangesCalls.ShouldBe(0);
    }

    [Fact]
    public async Task Update_of_a_missing_tournament_is_not_found()
    {
        var result = await new UpdateTournamentCommandHandler(_tournaments, _sanitizer, _storage, _unitOfWork)
            .Handle(new UpdateTournamentCommand { Id = Guid.CreateVersion7(), Name = "X", StartsAt = Now, EndsAt = Now }, CancellationToken.None);

        result.Error.ShouldBe(EsportsErrors.TournamentNotFound);
    }

    [Fact]
    public async Task Invalid_transition_is_a_refusal_and_nothing_is_saved()
    {
        var handler = new ChangeTournamentStatusCommandHandler(_tournaments, _unitOfWork);

        var finish = await handler.Handle(
            new ChangeTournamentStatusCommand(_world.Tournament.Id, TournamentTransition.Finish), CancellationToken.None);
        var start = await handler.Handle(
            new ChangeTournamentStatusCommand(_world.Tournament.Id, TournamentTransition.Start), CancellationToken.None);

        finish.Error.Code.ShouldBe("tournament.cannot_finish");
        start.IsSuccess.ShouldBeTrue();
        _world.Tournament.Status.ShouldBe(TournamentStatus.Ongoing);
        _unitOfWork.SaveChangesCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Team_awaiting_moderation_cannot_join_a_tournament()
    {
        var team = _world.AddUserTeam(Guid.CreateVersion7(), "Kokiri", "KOK");

        var result = await AddTeam(team.Id);

        result.Error.ShouldBe(EsportsErrors.TeamNotApproved);
        _world.Tournament.Participants.ShouldBeEmpty();
    }

    [Fact]
    public async Task Approved_team_joins_once()
    {
        var team = _world.AddTeam("Hyrule Knights", "HYR");

        (await AddTeam(team.Id)).IsSuccess.ShouldBeTrue();
        var again = await AddTeam(team.Id);

        again.Error.Code.ShouldBe("tournament.team_already_participates");
        _world.Tournament.Participants.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Team_with_matches_in_the_tournament_stays_in_the_roster()
    {
        var teamA = _world.AddTeam("Hyrule Knights", "HYR");
        var teamB = _world.AddTeam("Gerudo Valley", "GRD");
        _world.Tournament.AddTeam(teamA.Id, 1);
        _world.Tournament.AddTeam(teamB.Id, 2);
        _world.Schedule(teamA, teamB, Now.AddDays(1));

        var result = await new RemoveTournamentTeamCommandHandler(
                _tournaments, _world.Read(_world.Matches), new InMemoryQueryExecutor(), _unitOfWork)
            .Handle(new RemoveTournamentTeamCommand(_world.Tournament.Id, teamA.Id), CancellationToken.None);

        result.Error.ShouldBe(EsportsErrors.ParticipantHasMatches);
        _world.Tournament.Participants.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Tournament_with_matches_is_not_deleted()
    {
        var teamA = _world.AddTeam("Hyrule Knights", "HYR");
        var teamB = _world.AddTeam("Gerudo Valley", "GRD");
        _world.Schedule(teamA, teamB, Now.AddDays(1));

        var result = await Delete();

        result.Error.ShouldBe(EsportsErrors.TournamentHasMatches);
        _tournaments.Entities.ShouldContain(_world.Tournament);
    }

    [Fact]
    public async Task Tournament_without_history_is_deleted_with_its_logo()
    {
        _world.Tournament.ChangeImages("0123456789abcdef0123456789abcdef.png", null);

        var result = await Delete();

        result.IsSuccess.ShouldBeTrue();
        _tournaments.Entities.ShouldBeEmpty();
        _storage.Deleted.ShouldBe(["0123456789abcdef0123456789abcdef.png"]);
    }

    private Task<ZeldaArena.Domain.Common.Result> AddTeam(Guid teamId) =>
        new AddTournamentTeamCommandHandler(_tournaments, _world.Read(_world.Teams), new InMemoryQueryExecutor(), _unitOfWork)
            .Handle(new AddTournamentTeamCommand(_world.Tournament.Id, teamId, 1), CancellationToken.None);

    private Task<ZeldaArena.Domain.Common.Result> Delete() =>
        new DeleteTournamentCommandHandler(_tournaments, _world.Read(_world.Matches), new InMemoryQueryExecutor(), _storage, _unitOfWork)
            .Handle(new DeleteTournamentCommand(_world.Tournament.Id), CancellationToken.None);

    private static CreateTournamentCommand Command(string name) => new()
    {
        Name = name,
        StartsAt = Now,
        EndsAt = Now.AddDays(1),
    };
}