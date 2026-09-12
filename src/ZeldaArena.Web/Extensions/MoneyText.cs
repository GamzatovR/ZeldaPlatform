using System.Globalization;

namespace ZeldaArena.Web.Extensions;

public static class MoneyText
{
    public static string Format(decimal amount, string currency)
    {
        var format = decimal.Truncate(amount) == amount ? "N0" : "N2";

        return $"{amount.ToString(format, CultureInfo.CurrentCulture)} {currency}";
    }
}