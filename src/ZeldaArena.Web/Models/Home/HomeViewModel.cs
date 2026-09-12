using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.News.Queries.GetLatestNews;
using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;

namespace ZeldaArena.Web.Models.Home;

public sealed class HomeViewModel
{
    public required IReadOnlyList<MatchCardDto> Matches { get; init; }

    public required IReadOnlyList<NewsListItemDto> News { get; init; }

    public required IReadOnlyList<TeamListItemDto> TopTeams { get; init; }

    public required IReadOnlyList<ProductListItemDto> Products { get; init; }
}