using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Domain.Common;

public class ErrorTests
{
    private static readonly Error Stock = new("cart.insufficient_stock", "Мало на складе.");

    [Fact]
    public void Error_without_arguments_has_an_empty_list() =>
        Stock.Arguments.ShouldBeEmpty();

    [Fact]
    public void Arguments_are_attached_without_touching_the_original()
    {
        var concrete = Stock.WithArguments(3);

        concrete.Arguments.ShouldBe([3]);
        Stock.Arguments.ShouldBeEmpty();
        concrete.Code.ShouldBe(Stock.Code);
    }

    /// <summary>Две ошибки «осталось 3 шт.» — одна и та же ошибка, хоть списки и разные объекты.</summary>
    [Fact]
    public void Equality_compares_argument_values()
    {
        Stock.WithArguments(3).ShouldBe(Stock.WithArguments(3));
        Stock.WithArguments(3).GetHashCode().ShouldBe(Stock.WithArguments(3).GetHashCode());
    }

    [Fact]
    public void Different_arguments_make_different_errors() =>
        Stock.WithArguments(3).ShouldNotBe(Stock.WithArguments(2));

    [Fact]
    public void Errors_without_arguments_stay_equal() =>
        new Error("a", "b").ShouldBe(new Error("a", "b"));
}