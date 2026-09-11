namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Whitelist сортировок: строковый ключ из URL → готовое правило упорядочивания
/// (docs/SPEC.md §15). Имя поля из запроса никогда не попадает в выражение —
/// сортировать можно только тем, что заранее перечислено здесь.
///
/// Правило хранится функцией над <see cref="IQueryable{T}"/>, а не выражением поля:
/// так сортировка по нескольким полям и по вложенному объекту-значению
/// (<c>PrizePool.Amount</c>) записывается обычным LINQ и целиком уезжает в SQL.
/// </summary>
public sealed class SortMap<TEntity>
{
    private readonly Dictionary<string, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>> _rules =
        new(StringComparer.OrdinalIgnoreCase);

    // Порядок ключей — порядок пунктов в выпадающем списке сортировки. Словарь его
    // не гарантирует, поэтому ключи хранятся ещё и списком.
    private readonly List<string> _keys = [];

    private string? _defaultKey;

    public IReadOnlyList<string> Keys => _keys;

    /// <summary>Ключ, которым сортируется список, когда в URL сортировки нет или она неизвестна.</summary>
    public string DefaultKey => _defaultKey
        ?? throw new InvalidOperationException("В карте сортировок не задано правило по умолчанию.");

    public SortMap<TEntity> Add(
        string key,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> rule,
        bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(rule);

        _rules.Add(key, rule);
        _keys.Add(key);

        if (isDefault)
        {
            _defaultKey = key;
        }

        return this;
    }

    public bool IsKnown(string? key) =>
        !string.IsNullOrWhiteSpace(key) && _rules.ContainsKey(key);

    /// <summary>
    /// Ключ, которым список будет отсортирован на самом деле: известный — как есть,
    /// неизвестный или пустой — ключ по умолчанию. По нему форма отмечает выбранный пункт.
    /// </summary>
    public string Resolve(string? key) => IsKnown(key) ? _keys.First(known =>
        string.Equals(known, key, StringComparison.OrdinalIgnoreCase)) : DefaultKey;

    /// <summary>
    /// Применяет сортировку. Неизвестный ключ заменяется правилом по умолчанию:
    /// сохранённая ссылка обязана открыть список и после того, как
    /// сортировку переименовали или убрали, а не показывать страницу ошибки.
    /// </summary>
    public IOrderedQueryable<TEntity> Apply(IQueryable<TEntity> query, string? key)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rule = IsKnown(key) ? _rules[key!] : _rules[DefaultKey];

        return rule(query);
    }
}