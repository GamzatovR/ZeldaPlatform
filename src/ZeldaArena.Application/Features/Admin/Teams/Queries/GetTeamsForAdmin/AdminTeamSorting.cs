using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;

public static class AdminTeamSorting
{
    public const string NameAscending = "name_asc";
    public const string NameDescending = "name_desc";
    public const string RatingDescending = "rating_desc";
    public const string RatingAscending = "rating_asc";

    public static readonly SortMap<Team> Map = new SortMap<Team>()
        .Add(NameAscending, query => query.OrderBy(team => team.Name), isDefault: true)
        .Add(NameDescending, query => query.OrderByDescending(team => team.Name))
        .Add(RatingDescending, query => query.OrderByDescending(team => team.Rating).ThenBy(team => team.Name))
        .Add(RatingAscending, query => query.OrderBy(team => team.Rating).ThenBy(team => team.Name));
}