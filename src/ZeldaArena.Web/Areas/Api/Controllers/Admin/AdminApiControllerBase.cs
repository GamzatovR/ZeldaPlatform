using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Web.Areas.Admin.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers.Admin;

public abstract class AdminApiControllerBase(IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    protected string AdminPageUrl(string action, string controller, object? values = null)
    {
        var routeValues = new RouteValueDictionary(values) { ["area"] = AdminControllerBase.AreaName };

        return Url.Action(action, controller, routeValues)
            ?? throw new InvalidOperationException($"Нет маршрута к Admin/{controller}.{action}.");
    }
}