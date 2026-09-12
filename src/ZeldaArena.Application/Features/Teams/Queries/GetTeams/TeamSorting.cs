using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeams;

/// <summary>Разрешённые сортировки списка команд — whitelist из.</summary>
public static class TeamSorting
{
    public const string RatingDescending = "rating_desc";
    public const string RatingAscending = "rating_asc";
    public const string NameAscending = "name_asc";

    public static readonly SortMap<Team> Map = new SortMap<Team>()
        .Add(
            RatingDescending,
            query => query.OrderByDescending(team => team.Rating).ThenBy(team => team.Name),
            isDefault: true)
        .Add(RatingAscending, query => query.OrderBy(team => team.Rating).ThenBy(team => team.Name))
        .Add(NameAscending, query => query.OrderBy(team => team.Name));
}