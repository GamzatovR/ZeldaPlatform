using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

using ZeldaArena.Web.Areas.Admin.Controllers;
using ZeldaArena.Web.Areas.Api.Controllers.Admin;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Admin;

public sealed class AdminAreaConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (!IsAdmin(controller))
        {
            return;
        }

        // Порядок значим: сначала «вошёл ли и есть ли роль», потом «есть ли второй фактор».
        controller.Filters.Add(new AuthorizeFilter(PolicyNames.ModeratorOrAdmin));
        controller.Filters.Add(new AdminTwoFactorFilter());
    }

    public static bool IsAdmin(ControllerModel controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var type = controller.ControllerType;

        return typeof(AdminControllerBase).IsAssignableFrom(type)
            || typeof(AdminApiControllerBase).IsAssignableFrom(type)
            || (controller.RouteValues.TryGetValue("area", out var area)
                && string.Equals(area, AdminControllerBase.AreaName, StringComparison.OrdinalIgnoreCase));
    }
}