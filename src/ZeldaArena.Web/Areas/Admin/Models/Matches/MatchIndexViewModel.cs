using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;
using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchFormOptions;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

public sealed class MatchIndexViewModel
{
    public required GetMatchesForAdminQuery Filter { get; init; }

    public required PagedResult<AdminMatchRowDto> Result { get; init; }

    /// <summary>Турниры для фильтра; пусты в ответе API — там рисуется только таблица.</summary>
    public IReadOnlyList<AdminTournamentOptionDto> Tournaments { get; init; } = [];

    public string Sort => AdminMatchSorting.Map.Resolve(Filter.Sort);
}