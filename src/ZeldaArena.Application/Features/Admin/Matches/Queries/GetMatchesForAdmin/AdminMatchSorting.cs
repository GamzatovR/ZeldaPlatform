using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;

public static class AdminMatchSorting
{
    public const string DateDescending = "date_desc";
    public const string DateAscending = "date_asc";

    public static readonly SortMap<Match> Map = new SortMap<Match>()
        .Add(DateDescending, query => query.OrderByDescending(match => match.ScheduledAt), isDefault: true)
        .Add(DateAscending, query => query.OrderBy(match => match.ScheduledAt));
}