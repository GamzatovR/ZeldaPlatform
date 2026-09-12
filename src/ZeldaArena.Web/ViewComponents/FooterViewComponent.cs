using Microsoft.AspNetCore.Mvc;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>Подвал.</summary>
public sealed class FooterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}