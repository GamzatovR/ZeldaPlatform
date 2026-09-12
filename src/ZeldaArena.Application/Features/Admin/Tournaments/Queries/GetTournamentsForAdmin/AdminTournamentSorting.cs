using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;

/// <summary>
/// Whitelist сортировки таблицы турниров (§10.2): ключ из адреса выбирает готовое
/// выражение, имя поля в запрос не попадает никогда. Ключи — пары по возрастанию
/// и убыванию: их переключает <c>&lt;sortable-header&gt;</c>.
/// </summary>
public static class AdminTournamentSorting
{
    public const string DateDescending = "date_desc";
    public const string DateAscending = "date_asc";
    public const string NameAscending = "name_asc";
    public const string NameDescending = "name_desc";
    public const string PrizeAscending = "prize_asc";
    public const string PrizeDescending = "prize_desc";

    public static readonly SortMap<Tournament> Map = new SortMap<Tournament>()
        .Add(DateDescending, query => query.OrderByDescending(tournament => tournament.StartsAt), isDefault: true)
        .Add(DateAscending, query => query.OrderBy(tournament => tournament.StartsAt))
        .Add(NameAscending, query => query.OrderBy(tournament => tournament.Name))
        .Add(NameDescending, query => query.OrderByDescending(tournament => tournament.Name))
        .Add(PrizeAscending, query => query
            .OrderBy(tournament => tournament.PrizePool.Amount)
            .ThenByDescending(tournament => tournament.StartsAt))
        .Add(PrizeDescending, query => query
            .OrderByDescending(tournament => tournament.PrizePool.Amount)
            .ThenByDescending(tournament => tournament.StartsAt));
}