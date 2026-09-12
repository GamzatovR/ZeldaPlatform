using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Carts.Queries.GetMiniCart;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>Мини-корзина в шапке.</summary>
public sealed class MiniCartViewComponent(ISender sender) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() =>
        View(await sender.Send(new GetMiniCartQuery(), HttpContext.RequestAborted));
}