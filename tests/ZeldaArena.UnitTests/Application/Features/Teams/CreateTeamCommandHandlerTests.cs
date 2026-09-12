using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Teams.Commands.CreateTeam;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Teams;

public class CreateTeamCommandHandlerTests
{
    private static readonly Guid Subscriber = Guid.CreateVersion7();

    private readonly EsportsWorld _world = new();
    private readonly StubEntitlementService _entitlements = new StubEntitlementService()
        .Grant(Subscriber, FeatureCodes.TeamCreate);

    private readonly InMemoryRepository<Team> _written = new();
    private readonly InMemoryFileStorage _storage = new();
    private readonly RecordingUnitOfWork _unitOfWork = new();

    public CreateTeamCommandHandlerTests()
    {
        _world.AddTeam("Hyrule Knights", "HYR");
    }

    [Fact]
    public async Task Subscriber_creates_a_team_awaiting_moderation()
    {
        var result = await Handle(Command("Стражи Хайрула"));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("strazhi-khayrula");

        var team = _written.Entities.ShouldHaveSingleItem();
        team.OwnerUserId.ShouldBe(Subscriber);
        team.IsApproved.ShouldBeFalse();
        _unitOfWork.SaveChangesCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Without_the_feature_the_scenario_refuses_on_its_own()
    {
        _entitlements.Revoke(Subscriber, FeatureCodes.TeamCreate);

        var result = await Handle(Command("Стражи Хайрула"));

        result.Error.ShouldBe(EsportsErrors.FeatureRequired);
        _written.Entities.ShouldBeEmpty();
    }

    [Fact]
    public async Task Anonymous_caller_is_refused()
    {
        var result = await HandleAs(Command("Стражи Хайрула"), user: null);

        result.Error.ShouldBe(AccountErrors.UserNotFound);
    }

    /// <summary>Лимит по умолчанию — одна команда.</summary>
    [Fact]
    public async Task Second_team_exceeds_the_default_limit()
    {
        _world.AddUserTeam(Subscriber, "First Squad", "FST");

        var result = await Handle(Command("Second Squad"));

        result.Error.ShouldBe(EsportsErrors.TeamLimitReached);
    }

    /// <summary>Лимит — параметр фичи (EP-5): тариф с Value = 2 разрешает вторую команду.</summary>
    [Fact]
    public async Task Feature_value_raises_the_limit()
    {
        _entitlements.Grant(Subscriber, FeatureCodes.TeamCreate, value: "2");
        _world.AddUserTeam(Subscriber, "First Squad", "FST");

        var result = await Handle(Command("Second Squad"));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Name_already_in_use_is_refused_case_insensitively()
    {
        var result = await Handle(Command("HYRULE knights"));

        result.Error.ShouldBe(EsportsErrors.TeamNameTaken);
    }

    [Fact]
    public async Task Taken_slug_gets_a_number()
    {
        // Другое название, тот же слаг после нормализации.
        var result = await Handle(Command("Hyrule-Knights!"));

        result.Value.ShouldBe("hyrule-knights-2");
    }

    [Fact]
    public async Task Logo_is_stored_and_attached()
    {
        var result = await Handle(Command("Стражи Хайрула", SampleImages.Png));

        result.IsSuccess.ShouldBeTrue();
        var stored = _storage.Files.Keys.ShouldHaveSingleItem();
        _written.Entities.Single().LogoPath.ShouldBe(stored);
    }

    /// <summary>Исполняемый файл под именем logo.png отвергается по содержимому и на диск не попадает.</summary>
    [Fact]
    public async Task Executable_disguised_as_png_is_refused_and_never_stored()
    {
        var result = await Handle(Command("Стражи Хайрула", SampleImages.Executable));

        result.Error.ShouldBe(FileErrors.InvalidImage);
        _storage.Files.ShouldBeEmpty();
        _written.Entities.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("", "HYR", "RU")]
    [InlineData("Name", "H", "RU")]
    [InlineData("Name", "TOOLONGTAG", "RU")]
    [InlineData("Name", "H-R", "RU")]
    [InlineData("Name", "HYR", "Russia")]
    public void Validator_rejects_broken_profile_fields(string name, string tag, string country) =>
        new CreateTeamCommandValidator(new FixedDateTimeProvider(EsportsWorld.Now))
            .Validate(new CreateTeamCommand { Name = name, Tag = tag, CountryCode = country })
            .IsValid.ShouldBeFalse();

    [Fact]
    public void Validator_rejects_a_founding_date_in_the_future() =>
        new CreateTeamCommandValidator(new FixedDateTimeProvider(EsportsWorld.Now))
            .Validate(Command("Name") with { FoundedAt = DateOnly.FromDateTime(EsportsWorld.Now.UtcDateTime).AddDays(1) })
            .IsValid.ShouldBeFalse();

    [Fact]
    public void Validator_rejects_a_logo_with_a_foreign_extension() =>
        new CreateTeamCommandValidator(new FixedDateTimeProvider(EsportsWorld.Now))
            .Validate(Command("Name") with { Logo = new FileUpload(SampleImages.Png.AsStream(), "logo.exe", "image/png", 16) })
            .IsValid.ShouldBeFalse();

    [Fact]
    public void Audit_record_shows_the_file_not_the_stream() =>
        Command("Name", SampleImages.Png).Logo!.ToString().ShouldBe("logo.png (image/png, 16 B)");

    private static CreateTeamCommand Command(string name, byte[]? logo = null) => new()
    {
        Name = name,
        Tag = "HYR",
        CountryCode = "RU",
        Region = Region.Cis,
        Logo = logo is null ? null : new FileUpload(logo.AsStream(), "logo.png", "image/png", logo.Length),
    };

    private Task<Result<string>> Handle(CreateTeamCommand command) => HandleAs(command, Subscriber);

    private Task<Result<string>> HandleAs(CreateTeamCommand command, Guid? user)
    {
        var handler = new CreateTeamCommandHandler(
            _written,
            _world.Read(_world.Teams),
            new InMemoryQueryExecutor(),
            _entitlements,
            new StubCurrentUserService { UserId = user },
            _storage,
            _unitOfWork);

        return handler.Handle(command, CancellationToken.None);
    }
}