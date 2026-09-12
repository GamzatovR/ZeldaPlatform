namespace ZeldaArena.Application.Common.Models;

public abstract record FilterBase
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = PageSizes.Default;

    /// <summary>Ключ сортировки, например <c>prize_desc</c>. Разбирается по whitelist.</summary>
    public string? Sort { get; init; }

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSizes.Normalize(PageSize);
}