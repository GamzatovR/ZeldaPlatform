using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Web.Areas.Admin;
using ZeldaArena.Web.Areas.Admin.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers.Admin;

/// <summary>
/// Основа AJAX-эндпоинтов админки: <c>/api/admin/…</c> (docs/SPEC.md §10.1, сценарий 12).
///
/// Живут они в <c>Areas/Api</c>, а не в области Admin: транспорт тот же, что у всего API —
/// антифоржери, <c>ProblemDetails</c>, 401/403 вместо редиректа. Доступ вешает то же
/// <see cref="AdminAreaConvention"/>, что и на страницы админки, — по этому базовому
/// классу, поэтому правила доступа к странице и к её эндпоинтам не могут разойтись.
/// </summary>
public abstract class AdminApiControllerBase(IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    /// <summary>
    /// Адрес страницы админки, которой принадлежит список: на него ведут ссылки
    /// пагинации и сортировки в partial, отданном из <c>/api/admin/…</c>.
    /// </summary>
    protected string AdminPageUrl(string action, string controller, object? values = null)
    {
        var routeValues = new RouteValueDictionary(values) { ["area"] = AdminControllerBase.AreaName };

        return Url.Action(action, controller, routeValues)
            ?? throw new InvalidOperationException($"Нет маршрута к Admin/{controller}.{action}.");
    }
}