using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

public sealed class GetUsersForAdminQueryHandler(IUserAdministrationService users)
    : IRequestHandler<GetUsersForAdminQuery, PagedResult<AdminUserRowDto>>
{
    public Task<PagedResult<AdminUserRowDto>> Handle(
        GetUsersForAdminQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return users.SearchAsync(
            new AdminUserFilter(
                request.Search,
                request.Role,
                request.IsBlocked,
                AdminUserSorting.Resolve(request.Sort) == AdminUserSorting.NewestFirst,
                request.NormalizedPage,
                request.NormalizedPageSize),
            cancellationToken);
    }
}