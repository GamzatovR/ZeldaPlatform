using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.ValueObjects;

public class CountryCodeTests
{
    [Theory]
    [InlineData("ru", "RU")]
    [InlineData("SE", "SE")]
    [InlineData(" de ", "DE")]
    public void Code_is_normalized(string input, string expected)
    {
        CountryCode.From(input).Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("RUS")]
    [InlineData("R")]
    [InlineData("R1")]
    public void Wrong_format_is_rejected(string input)
    {
        Should.Throw<InvariantViolationException>(() => CountryCode.From(input))
            .Code.ShouldBe("country_code.invalid");
    }

    [Fact]
    public void Equality_is_structural()
    {
        CountryCode.From("ru").ShouldBe(CountryCode.From("RU"));
    }
}