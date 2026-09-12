using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.News.Commands.RegisterNewsView;
using ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;

namespace ZeldaArena.Web.Controllers;

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