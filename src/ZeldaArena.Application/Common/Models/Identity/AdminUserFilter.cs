namespace ZeldaArena.Application.Common.Models.Identity;

public sealed record AdminUserFilter(
    string? Search,
    string? Role,
    bool? IsBlocked,
    bool NewestFirst,
    int Page,
    int PageSize);