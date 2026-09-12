using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayersForAdmin;

namespace ZeldaArena.Web.Areas.Admin.Models.Players;

public sealed class PlayerIndexViewModel
{
    public required GetPlayersForAdminQuery Filter { get; init; }

    public required PagedResult<AdminPlayerRowDto> Result { get; init; }

    public string Sort => AdminPlayerSorting.Map.Resolve(Filter.Sort);
}