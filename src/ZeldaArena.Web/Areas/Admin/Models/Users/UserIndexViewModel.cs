using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

namespace ZeldaArena.Web.Areas.Admin.Models.Users;

public sealed class UserIndexViewModel
{
    public required GetUsersForAdminQuery Filter { get; init; }

    public required PagedResult<AdminUserRowDto> Result { get; init; }

    public string Sort => AdminUserSorting.Resolve(Filter.Sort);
}