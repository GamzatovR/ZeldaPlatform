using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;

namespace ZeldaArena.Web.Models.Teams;

/// <summary>Список команд (docs/SPEC.md §9.3, п. 6): фильтр из адреса и страница результата.</summary>
public sealed class TeamListViewModel
{
    public required GetTeamsQuery Filter { get; init; }

    public required PagedResult<TeamListItemDto> Result { get; init; }
}