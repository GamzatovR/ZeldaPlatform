namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Общая часть любого фильтруемого запроса: страница, размер страницы и ключ сортировки
/// (docs/SPEC.md §10.2). Значения приходят из query-string, поэтому наружу отдаются
/// нормализованными — хендлер не должен помнить про проверку каждый раз.
/// </summary>
public abstract record FilterBase
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = PageSizes.Default;

    /// <summary>Ключ сортировки, например <c>prize_desc</c>. Разбирается по whitelist.</summary>
    public string? Sort { get; init; }

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSizes.Normalize(PageSize);
}