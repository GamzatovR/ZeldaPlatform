using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.ValueObjects;

/// <summary>
/// Код страны ISO 3166-1 alpha-2 («RU», «SE»). Используется у команд и игроков
/// для флагов и фильтра по стране (docs/SPEC.md §9.3).
/// </summary>
public sealed class CountryCode : ValueObject
{
    public const int Length = 2;

    private CountryCode(string value) => Value = value;

    public string Value { get; }

    public static CountryCode From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();

        InvariantViolationException.ThrowIf(
            normalized.Length != Length || !normalized.All(char.IsAsciiLetterUpper),
            "country_code.invalid",
            $"Код страны — две латинские буквы по ISO 3166-1 alpha-2, получено «{value}».");

        return new CountryCode(normalized);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}