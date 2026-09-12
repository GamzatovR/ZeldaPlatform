using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Account;

/// <summary>Remote-проверка занятости адреса на форме регистрации.</summary>
public class EmailAvailabilityTests
{
    private const string Email = "player@zeldaarena.test";

    private readonly InMemoryUserAccountService _accounts = new();

    [Fact]
    public async Task Unknown_address_is_available()
    {
        _accounts.Add("someone-else@zeldaarena.test");

        (await Check(Email)).ShouldBeTrue();
    }

    [Fact]
    public async Task Registered_address_is_taken()
    {
        _accounts.Add(Email);

        (await Check(Email)).ShouldBeFalse();
    }

    [Fact]
    public async Task Surrounding_spaces_do_not_hide_a_taken_address()
    {
        _accounts.Add(Email);

        (await Check($"  {Email} ")).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_address_is_rejected(string email)
    {
        new IsEmailAvailableQueryValidator().Validate(new IsEmailAvailableQuery(email)).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Overlong_address_is_rejected()
    {
        var email = new string('a', AccountValidationRules.MaxEmailLength) + "@zeldaarena.test";

        new IsEmailAvailableQueryValidator().Validate(new IsEmailAvailableQuery(email)).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Malformed_address_passes_the_validator()
    {
        new IsEmailAvailableQueryValidator().Validate(new IsEmailAvailableQuery("not-an-email")).IsValid.ShouldBeTrue();
    }

    private Task<bool> Check(string email) =>
        new IsEmailAvailableQueryHandler(_accounts).Handle(new IsEmailAvailableQuery(email), CancellationToken.None);
}