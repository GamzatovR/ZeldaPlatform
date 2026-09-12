using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;

namespace ZeldaArena.Web.Areas.Admin.Models.Tournaments;

/// <summary>
/// Таблица турниров. Фильтр едет в partial вместе с результатом: по нему заголовки
/// знают текущую сортировку — и на странице, и в ответе <c>/api/admin/tournaments</c>.
/// </summary>
public sealed class TournamentIndexViewModel
{
    public required GetTournamentsForAdminQuery Filter { get; init; }

    public required PagedResult<AdminTournamentRowDto> Result { get; init; }

    public string Sort => AdminTournamentSorting.Map.Resolve(Filter.Sort);
}