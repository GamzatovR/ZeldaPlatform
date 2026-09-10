using Microsoft.AspNetCore.Mvc;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>
/// Подвал (docs/SPEC.md §10.2). Данных не требует, но остаётся ViewComponent,
/// а не partial: в Фазе 11 сюда приходит переключатель языка, которому нужен
/// список поддерживаемых культур, и менять тип компонента тогда не придётся.
/// </summary>
public sealed class FooterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}