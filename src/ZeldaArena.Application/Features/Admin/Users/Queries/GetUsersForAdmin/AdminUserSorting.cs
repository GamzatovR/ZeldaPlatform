namespace ZeldaArena.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

public static class AdminUserSorting
{
    public const string EmailAscending = "email_asc";
    public const string NewestFirst = "created_desc";

    public static string Resolve(string? sort) =>
        string.Equals(sort, NewestFirst, StringComparison.OrdinalIgnoreCase)
            ? NewestFirst
            : EmailAscending;
}