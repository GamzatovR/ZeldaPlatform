using System.Diagnostics;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;
using ZeldaArena.Application.Features.News.Queries.GetLatestNews;
using ZeldaArena.Web.Models;
using ZeldaArena.Web.Models.Home;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Контроллер принимает только <c>ISender</c> (docs/SPEC.md §5.2, правило 4)
/// и умеет ровно одно: отправить запрос и выбрать представление.
/// </summary>
public class HomeController(ISender sender) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Два независимых запроса. Параллелить их нельзя: DbContext не потокобезопасен,
        // а оба уходят через один и тот же scoped-контекст.
        var matches = await sender
            .Send(new GetHomeMatchesQuery(), cancellationToken)
            .ConfigureAwait(false);

        var news = await sender
            .Send(new GetLatestNewsQuery(), cancellationToken)
            .ConfigureAwait(false);

        return View(new HomeViewModel { Matches = matches, News = news });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}