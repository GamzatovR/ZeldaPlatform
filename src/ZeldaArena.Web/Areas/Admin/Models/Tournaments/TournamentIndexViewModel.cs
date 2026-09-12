using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;

namespace ZeldaArena.Web.Areas.Admin.Models.Tournaments;

public sealed class TournamentIndexViewModel
{
    public required GetTournamentsForAdminQuery Filter { get; init; }

    public required PagedResult<AdminTournamentRowDto> Result { get; init; }

    public string Sort => AdminTournamentSorting.Map.Resolve(Filter.Sort);
}