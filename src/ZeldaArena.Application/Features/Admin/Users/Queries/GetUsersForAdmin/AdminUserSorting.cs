namespace ZeldaArena.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

/// <summary>
/// Ключи сортировки пользователей. Выборку собирает Infrastructure, поэтому здесь
/// не <c>SortMap</c>, а whitelist ключей: ключ превращается в флаг порядка и дальше
/// имя поля из адреса никуда не идёт.
/// </summary>
public static class AdminUserSorting
{
    public const string EmailAscending = "email_asc";
    public const string NewestFirst = "created_desc";

    public static string Resolve(string? sort) =>
        string.Equals(sort, NewestFirst, StringComparison.OrdinalIgnoreCase)
            ? NewestFirst
            : EmailAscending;
}