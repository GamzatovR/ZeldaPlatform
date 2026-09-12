using ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.UnitTests.Application.Features;

public class SliceValidatorTests
{
    private static readonly GetTournamentsQueryValidator Tournaments = new();
    private static readonly UpdateMatchScoreCommandValidator Matches = new();

    [Fact]
    public void Default_tournaments_query_is_valid() =>
        Tournaments.Validate(new GetTournamentsQuery()).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_below_one_is_rejected(int page) =>
        Tournaments.Validate(new GetTournamentsQuery { Page = page })
            .IsValid.ShouldBeFalse();

    [Fact]
    public void Too_long_search_is_rejected() =>
        Tournaments.Validate(new GetTournamentsQuery
        {
            Search = new string('z', GetTournamentsQueryValidator.MaxSearchLength + 1),
        }).IsValid.ShouldBeFalse();

    [Fact]
    public void Status_outside_the_enum_is_rejected() =>
        Tournaments.Validate(new GetTournamentsQuery { Status = (TournamentStatus)99 })
            .IsValid.ShouldBeFalse();

    [Fact]
    public void Unusual_page_size_and_sort_do_not_fail_validation() =>
        Tournaments.Validate(new GetTournamentsQuery { PageSize = 37, Sort = "whatever" })
            .IsValid.ShouldBeTrue();

    [Fact]
    public void Valid_score_command_passes() =>
        Matches.Validate(new UpdateMatchScoreCommand(Guid.CreateVersion7(), 2, 1))
            .IsValid.ShouldBeTrue();

    [Fact]
    public void Command_without_match_is_rejected() =>
        Matches.Validate(new UpdateMatchScoreCommand(Guid.Empty, 1, 0))
            .IsValid.ShouldBeFalse();

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(UpdateMatchScoreCommandValidator.MaxScore + 1, 0)]
    public void Score_outside_sane_bounds_is_rejected(int scoreA, int scoreB) =>
        Matches.Validate(new UpdateMatchScoreCommand(Guid.CreateVersion7(), scoreA, scoreB))
            .IsValid.ShouldBeFalse();

    [Fact]
    public void Series_format_is_not_the_validators_business() =>
        Matches.Validate(new UpdateMatchScoreCommand(Guid.CreateVersion7(), 5, 5))
            .IsValid.ShouldBeTrue();
}