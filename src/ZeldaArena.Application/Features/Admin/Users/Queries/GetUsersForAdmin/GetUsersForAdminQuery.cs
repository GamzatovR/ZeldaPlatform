using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

/// <summary>Таблица пользователей — <c>/admin/users</c> (docs/SPEC.md §9.4, п. 10).</summary>
public sealed record GetUsersForAdminQuery : FilterBase, IQuery<PagedResult<AdminUserRowDto>>
{
    public string? Search { get; init; }

    public string? Role { get; init; }

    public bool? IsBlocked { get; init; }
}