using System.Globalization;

using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.ValueObjects;

/// <summary>
/// Денежная сумма с валютой. Не бывает отрицательной, валюты не смешиваются
/// (docs/SPEC.md §5.3). В БД раскладывается на два столбца через ComplexProperty,
/// отдельной таблицы нет (docs/adr/ADR-0003).
/// </summary>
public sealed class Money : ValueObject
{
    public const string DefaultCurrency = "RUB";

    /// <summary>Столько знаков после запятой хранит <c>numeric(18,2)</c>.</summary>
    public const int Scale = 2;

    public Money(decimal amount, string currency)
    {
        var normalizedCurrency = NormalizeCurrency(currency);

        InvariantViolationException.ThrowIf(
            amount < 0m,
            "money.negative_amount",
            $"Денежная сумма не может быть отрицательной: {amount.ToString(CultureInfo.InvariantCulture)}.");

        InvariantViolationException.ThrowIf(
            decimal.Round(amount, Scale) != amount,
            "money.too_many_decimals",
            $"Сумма хранится с точностью до {Scale} знаков: {amount.ToString(CultureInfo.InvariantCulture)}.");

        Amount = amount;
        Currency = normalizedCurrency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public bool IsZero => Amount == 0m;

    public static Money Zero(string currency = DefaultCurrency) => new(0m, currency);

    public static Money FromRubles(decimal amount) => new(amount, DefaultCurrency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Вычитание. Уход в минус запрещён конструктором: скидка не может превысить сумму.
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(int factor)
    {
        InvariantViolationException.ThrowIf(
            factor < 0,
            "money.negative_factor",
            $"Множитель не может быть отрицательным: {factor}.");

        return new Money(Amount * factor, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public static Money operator *(Money money, int factor) => money.Multiply(factor);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public int CompareTo(Money other)
    {
        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public override string ToString() =>
        $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    private void EnsureSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        InvariantViolationException.ThrowIf(
            !string.Equals(Currency, other.Currency, StringComparison.Ordinal),
            "money.currency_mismatch",
            $"Нельзя смешивать валюты {Currency} и {other.Currency}.");
    }

    private static string NormalizeCurrency(string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var normalized = currency.Trim().ToUpperInvariant();

        InvariantViolationException.ThrowIf(
            normalized.Length != 3 || !normalized.All(char.IsAsciiLetterUpper),
            "money.invalid_currency",
            $"Код валюты — три латинские буквы по ISO 4217, получено «{currency}».");

        return normalized;
    }
}
