using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Carts.Queries.GetMiniCart;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>
/// Мини-корзина в шапке (docs/SPEC.md §10.2): ссылка на корзину и счётчик штук.
/// Счётчик рисует сервер на каждой странице, а после AJAX-добавления его обновляет
/// cart.js из ответа (сценарий 5 §10.1) без перезагрузки.
/// </summary>
public sealed class MiniCartViewComponent(ISender sender) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() =>
        View(await sender.Send(new GetMiniCartQuery(), HttpContext.RequestAborted));
}