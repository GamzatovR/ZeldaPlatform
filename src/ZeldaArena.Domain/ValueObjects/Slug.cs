using System.Text;

using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.ValueObjects;

/// <summary>Часть URL, однозначно указывающая на сущность.</summary>
public sealed class Slug : ValueObject
{
    public const int MaxLength = 128;

    private Slug(string value) => Value = value;

    public string Value { get; }

    /// <summary>Нормализует произвольный текст.</summary>
    public static Slug From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var builder = new StringBuilder(value.Length);
        foreach (var symbol in value.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterLower(symbol) || char.IsAsciiDigit(symbol))
            {
                builder.Append(symbol);
            }
            else if (symbol is ' ' or '-' or '_' or '.' or '/' && builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var normalized = builder.ToString().Trim('-');

        InvariantViolationException.ThrowIf(
            normalized.Length == 0,
            "slug.empty",
            $"Из строки «{value}» не получается слаг: не осталось ни одного допустимого символа.");

        InvariantViolationException.ThrowIf(
            normalized.Length > MaxLength,
            "slug.too_long",
            $"Слаг длиннее {MaxLength} символов.");

        return new Slug(normalized);
    }

    public static bool TryFrom(string? value, out Slug? slug)
    {
        try
        {
            slug = From(value!);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or DomainException)
        {
            slug = null;
            return false;
        }
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}