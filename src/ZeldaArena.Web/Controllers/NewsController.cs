using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.News.Commands.RegisterNewsView;
using ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Страница новости (docs/SPEC.md §9.3, п. 11). Отдельной ленты нет — новость
/// открывается из карточки на главной (решение Фазы 6).
/// </summary>
[Route("news")]
public sealed class NewsController(ISender sender) : Controller
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var article = await sender.Send(new GetNewsArticleBySlugQuery(slug), cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        await sender.Send(new RegisterNewsViewCommand(article.Id), cancellationToken);

        return View(article);
    }
}