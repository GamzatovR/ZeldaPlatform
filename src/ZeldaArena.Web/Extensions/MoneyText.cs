using System.Globalization;

namespace ZeldaArena.Web.Extensions;

/// <summary>
/// Сумма для показа: в текущей культуре (docs/SPEC.md §9.5), копейки — только
/// когда они есть. «6 990 RUB» читается лучше, чем «6 990,00 RUB», а «299,50 RUB»
/// не должно округлиться до «300».
/// </summary>
public static class MoneyText
{
    public static string Format(decimal amount, string currency)
    {
        var format = decimal.Truncate(amount) == amount ? "N0" : "N2";

        return $"{amount.ToString(format, CultureInfo.CurrentCulture)} {currency}";
    }
}