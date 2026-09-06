using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Amount_and_currency_are_kept_as_given()
    {
        var money = new Money(299m, "RUB");

        money.Amount.ShouldBe(299m);
        money.Currency.ShouldBe("RUB");
    }

    [Fact]
    public void Currency_is_normalized_to_upper_case()
    {
        new Money(10m, "rub").Currency.ShouldBe("RUB");
    }

    [Fact]
    public void Negative_amount_is_rejected()
    {
        var exception = Should.Throw<InvariantViolationException>(() => new Money(-1m, "RUB"));

        exception.Code.ShouldBe("money.negative_amount");
    }

    [Fact]
    public void More_than_two_decimals_is_rejected()
    {
        Should.Throw<InvariantViolationException>(() => new Money(10.005m, "RUB"))
            .Code.ShouldBe("money.too_many_decimals");
    }

    [Theory]
    [InlineData("RUBLE")]
    [InlineData("R")]
    [InlineData("R1B")]
    public void Currency_must_be_three_letters(string currency)
    {
        Should.Throw<InvariantViolationException>(() => new Money(10m, currency))
            .Code.ShouldBe("money.invalid_currency");
    }

    [Fact]
    public void Sum_of_same_currency_is_allowed()
    {
        (new Money(100m, "RUB") + new Money(199m, "RUB")).ShouldBe(new Money(299m, "RUB"));
    }

    [Fact]
    public void Mixing_currencies_is_rejected()
    {
        Should.Throw<InvariantViolationException>(() => new Money(100m, "RUB") + new Money(1m, "USD"))
            .Code.ShouldBe("money.currency_mismatch");
    }

    [Fact]
    public void Subtraction_below_zero_is_rejected()
    {
        // Скидка не может превысить сумму заказа (docs/SPEC.md §15).
        Should.Throw<InvariantViolationException>(() => new Money(100m, "RUB") - new Money(101m, "RUB"))
            .Code.ShouldBe("money.negative_amount");
    }

    [Fact]
    public void Multiplication_by_quantity_works()
    {
        (new Money(1500m, "RUB") * 3).ShouldBe(new Money(4500m, "RUB"));
    }

    [Fact]
    public void Comparison_uses_amount()
    {
        (new Money(10m, "RUB") < new Money(20m, "RUB")).ShouldBeTrue();
        (new Money(20m, "RUB") >= new Money(20m, "RUB")).ShouldBeTrue();
    }

    [Fact]
    public void Equality_is_structural()
    {
        new Money(10m, "RUB").ShouldBe(new Money(10m, "RUB"));
        new Money(10m, "RUB").ShouldNotBe(new Money(10m, "USD"));
        new Money(10m, "RUB").GetHashCode().ShouldBe(new Money(10m, "RUB").GetHashCode());
    }

    [Fact]
    public void Zero_defaults_to_rubles()
    {
        Money.Zero().ShouldBe(new Money(0m, "RUB"));
        Money.Zero().IsZero.ShouldBeTrue();
    }
}