using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

using ZeldaArena.Web.Areas.Admin.Controllers;
using ZeldaArena.Web.Areas.Api.Controllers.Admin;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Admin;

/// <summary>
/// Доступ ко всей админке в одном месте (docs/SPEC.md §8.1: «доступ в Area Admin —
/// через соглашение на всю область»).
///
/// Каждый контроллер области и каждый эндпоинт <c>/api/admin</c> получает политику
/// <see cref="PolicyNames.ModeratorOrAdmin"/> и фильтр второго фактора для администратора
/// (§8.2). Разделы, закрытые и для модератора (пользователи, тарифы, магазин), добавляют
/// свою политику атрибутом на контроллер; политики складываются по «И», поэтому атрибут
/// может только сузить доступ, но не расширить его.
///
/// Цель соглашения — наследники <see cref="AdminControllerBase"/> и
/// <see cref="AdminApiControllerBase"/>, а заодно любой контроллер с областью Admin:
/// архитектурный тест требует базовый класс, а здесь страховка на случай, если
/// тест однажды ослабят.
/// </summary>
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