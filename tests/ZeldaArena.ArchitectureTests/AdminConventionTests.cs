using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Web.Areas.Admin.Controllers;
using ZeldaArena.Web.Areas.Api.Controllers.Admin;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.ArchitectureTests;

public class AdminConventionTests
{
    private static readonly Assembly Web = typeof(AdminControllerBase).Assembly;

    [Fact]
    public void Admin_pages_derive_from_the_admin_controller_base()
    {
        var offenders = Web.GetTypes()
            .Where(type => type.Namespace?.StartsWith("ZeldaArena.Web.Areas.Admin.Controllers", StringComparison.Ordinal) == true)
            .Where(IsController)
            .Where(type => !typeof(AdminControllerBase).IsAssignableFrom(type))
            .Select(type => type.Name)
            .ToArray();

        offenders.ShouldBeEmpty(
            "Контроллер админки обязан наследовать AdminControllerBase — иначе соглашение "
            + $"не повесит на него политику и второй фактор. Нарушения: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Admin_endpoints_derive_from_the_admin_api_controller_base()
    {
        var offenders = Web.GetTypes()
            .Where(type => type.Namespace == "ZeldaArena.Web.Areas.Api.Controllers.Admin")
            .Where(IsController)
            .Where(type => !typeof(AdminApiControllerBase).IsAssignableFrom(type))
            .Select(type => type.Name)
            .ToArray();

        offenders.ShouldBeEmpty(
            "Эндпоинт /api/admin обязан наследовать AdminApiControllerBase: у страницы "
            + $"и её запросов должны быть одни правила доступа. Нарушения: {string.Join(", ", offenders)}");
    }

    [Theory]
    [InlineData("UsersController", PolicyNames.AdminOnly)]
    [InlineData("ProductsController", PolicyNames.AdminOnly)]
    [InlineData("OrdersController", PolicyNames.AdminOnly)]
    [InlineData("PlansController", PolicyNames.CanManageBilling)]
    [InlineData("FeaturesController", PolicyNames.CanManageBilling)]
    public void Restricted_sections_declare_their_policy(string controller, string policy)
    {
        var type = Web.GetTypes().Single(candidate =>
            candidate.Name == controller
            && candidate.Namespace?.StartsWith("ZeldaArena.Web.Areas.Admin.Controllers", StringComparison.Ordinal) == true);

        var declared = type.GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .Select(attribute => attribute.Policy)
            .ToArray();

        declared.ShouldContain(policy, $"{controller} обязан объявить политику {policy}.");
    }

    [Fact]
    public void Admin_area_is_declared_once_on_the_base_class()
    {
        typeof(AdminControllerBase).GetCustomAttribute<AreaAttribute>(inherit: false)
            .ShouldNotBeNull()
            .RouteValue.ShouldBe(AdminControllerBase.AreaName);

        var offenders = Web.GetTypes()
            .Where(type => type.Namespace?.StartsWith("ZeldaArena.Web.Areas.Admin.Controllers", StringComparison.Ordinal) == true)
            .Where(type => type != typeof(AdminControllerBase))
            .Where(type => type.GetCustomAttribute<AreaAttribute>(inherit: false) is not null)
            .Select(type => type.Name)
            .ToArray();

        offenders.ShouldBeEmpty($"Область объявляет AdminControllerBase. Нарушения: {string.Join(", ", offenders)}");
    }

    private static bool IsController(Type type) =>
        type.IsClass
        && !type.IsAbstract
        && type.GetCustomAttribute<CompilerGeneratedAttribute>() is null
        && type.Name.EndsWith("Controller", StringComparison.Ordinal);
}