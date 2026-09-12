namespace ZeldaArena.Application.Common.Models;

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

    public string Resolve(string? key) => IsKnown(key) ? _keys.First(known =>
        string.Equals(known, key, StringComparison.OrdinalIgnoreCase)) : DefaultKey;

    public IOrderedQueryable<TEntity> Apply(IQueryable<TEntity> query, string? key)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rule = IsKnown(key) ? _rules[key!] : _rules[DefaultKey];

        return rule(query);
    }
}