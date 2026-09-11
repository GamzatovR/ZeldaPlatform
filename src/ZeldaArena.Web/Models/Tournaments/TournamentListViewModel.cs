using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;

namespace ZeldaArena.Web.Models.Tournaments;

/// <summary>
/// Список турниров (docs/SPEC.md §9.3, п. 3). Фильтр — тот самый запрос, что ушёл
/// в хендлер: форма заполняется из него, поэтому после F5 и по пересланной ссылке
/// поля стоят ровно в том состоянии, которое описывает адрес (§10.2).
/// </summary>
public sealed class TournamentListViewModel
{
    public required GetTournamentsQuery Filter { get; init; }

    public required PagedResult<TournamentListItemDto> Result { get; init; }
}