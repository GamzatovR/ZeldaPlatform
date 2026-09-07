namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Страница результатов серверной пагинации (docs/SPEC.md §10.3). Кроме самих записей
/// несёт всё, что нужно tag helper'у пагинации и восстановлению состояния из URL.
/// </summary>
public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    /// <summary>
    /// Пустой результат — это одна пустая страница, а не ноль страниц: иначе разметка
    /// пагинации показывает «страница 1 из 0».
    /// </summary>
    public int TotalPages => TotalCount == 0
        ? 1
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Empty(int page, int pageSize) => new([], page, pageSize, 0);
}