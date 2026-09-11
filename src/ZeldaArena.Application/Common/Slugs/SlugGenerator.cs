using System.Text;

using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Common.Slugs;

/// <summary>
/// Слаг для сущности, которую создаёт пользователь: команды подписчика и её игроков.
///
/// <see cref="Slug.From"/> оставляет только латиницу и цифры, поэтому «Стражи Хайрула»
/// превратились бы в пустую строку и команда не создалась бы вовсе. Кириллица
/// транслитерируется, а если и после этого ничего не осталось (название из одних
/// иероглифов или эмодзи), берётся запасной текст — тег команды или ник.
///
/// Уникальность проверяет вызывающий через функцию <c>isTaken</c>: генератор
/// не знает, в какой таблице искать, и не должен.
/// </summary>
public static class SlugGenerator
{
    /// <summary>Сколько номеров перебирать до случайного суффикса.</summary>
    private const int MaxNumberedAttempts = 50;

    /// <summary>Место под суффикс «-50» или «-a1b2c3»; основа обрезается заранее.</summary>
    private const int SuffixReserve = 8;

    /// <summary>
    /// Слаги, совпадающие с маршрутами: <c>/teams/create</c> — страница создания,
    /// и команда с таким слагом стала бы недостижима.
    /// </summary>
    private static readonly HashSet<string> Reserved = new(StringComparer.Ordinal) { "create", "edit", "new" };

    private static readonly Dictionary<char, string> Cyrillic = new()
    {
        ['а'] = "a",
        ['б'] = "b",
        ['в'] = "v",
        ['г'] = "g",
        ['д'] = "d",
        ['е'] = "e",
        ['ё'] = "e",
        ['ж'] = "zh",
        ['з'] = "z",
        ['и'] = "i",
        ['й'] = "y",
        ['к'] = "k",
        ['л'] = "l",
        ['м'] = "m",
        ['н'] = "n",
        ['о'] = "o",
        ['п'] = "p",
        ['р'] = "r",
        ['с'] = "s",
        ['т'] = "t",
        ['у'] = "u",
        ['ф'] = "f",
        ['х'] = "kh",
        ['ц'] = "ts",
        ['ч'] = "ch",
        ['ш'] = "sh",
        ['щ'] = "shch",
        ['ъ'] = string.Empty,
        ['ы'] = "y",
        ['ь'] = string.Empty,
        ['э'] = "e",
        ['ю'] = "yu",
        ['я'] = "ya",
    };

    public static string Transliterate(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var builder = new StringBuilder(text.Length * 2);

        foreach (var symbol in text.ToLowerInvariant())
        {
            builder.Append(Cyrillic.TryGetValue(symbol, out var latin) ? latin : symbol.ToString());
        }

        return builder.ToString();
    }

    /// <summary>
    /// Первый свободный слаг: сама основа, затем основа с номером, в крайнем случае —
    /// со случайным суффиксом. Бросает, если ни из текста, ни из запасного текста
    /// слаг не складывается — валидатор сценария такого не пропускает.
    /// </summary>
    public static async Task<Slug> UniqueAsync(
        string text,
        string fallback,
        Func<Slug, CancellationToken, Task<bool>> isTaken,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(isTaken);

        var stem = Stem(text) ?? Stem(fallback)
            ?? throw new ArgumentException("Из названия не складывается адрес страницы.", nameof(text));

        var candidate = Slug.From(stem);

        for (var number = 2; number <= MaxNumberedAttempts + 1; number++)
        {
            if (!Reserved.Contains(candidate.Value)
                && !await isTaken(candidate, cancellationToken).ConfigureAwait(false))
            {
                return candidate;
            }

            candidate = Slug.From($"{stem}-{number}");
        }

        return Slug.From($"{stem}-{Guid.NewGuid().ToString("N")[..6]}");
    }

    private static string? Stem(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || !Slug.TryFrom(Transliterate(text), out var slug) || slug is null)
        {
            return null;
        }

        var value = slug.Value;

        return value.Length > Slug.MaxLength - SuffixReserve
            ? value[..(Slug.MaxLength - SuffixReserve)].TrimEnd('-')
            : value;
    }
}