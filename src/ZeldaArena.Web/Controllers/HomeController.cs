using System.Diagnostics;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;
using ZeldaArena.Application.Features.News.Queries.GetLatestNews;
using ZeldaArena.Application.Features.Shop.Queries.GetShowcaseProducts;
using ZeldaArena.Application.Features.Teams.Queries.GetTopTeams;
using ZeldaArena.Web.Models;
using ZeldaArena.Web.Models.Home;

namespace ZeldaArena.Web.Controllers;

public class HomeController(ISender sender) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Независимые запросы. Параллелить их нельзя: DbContext не потокобезопасен,
        // а оба уходят через один и тот же scoped-контекст.
        var matches = await sender
            .Send(new GetHomeMatchesQuery(), cancellationToken)
            .ConfigureAwait(false);

        var news = await sender
            .Send(new GetLatestNewsQuery(), cancellationToken)
            .ConfigureAwait(false);

        var teams = await sender
            .Send(new GetTopTeamsQuery(), cancellationToken)
            .ConfigureAwait(false);

        var products = await sender
            .Send(new GetShowcaseProductsQuery(), cancellationToken)
            .ConfigureAwait(false);

        return View(new HomeViewModel { Matches = matches, News = news, TopTeams = teams, Products = products });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}