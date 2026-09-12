using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;

namespace ZeldaArena.Web.Areas.Admin.Models.Teams;

public sealed class TeamIndexViewModel
{
    public required GetTeamsForAdminQuery Filter { get; init; }

    public required PagedResult<AdminTeamRowDto> Result { get; init; }

    public string Sort => AdminTeamSorting.Map.Resolve(Filter.Sort);
}