using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Web.Areas.Api.Controllers;

namespace ZeldaArena.ArchitectureTests;

public class ApiConventionTests
{
    private const string ApiNamespace = "ZeldaArena.Web.Areas.Api";

    [Fact]
    public void Api_controllers_should_derive_from_api_controller_base()
    {
        var offenders = ApiControllers()
            .Where(type => !type.IsSubclassOf(typeof(ApiControllerBase)))
            .Select(type => type.Name)
            .ToArray();

        offenders.ShouldBeEmpty(
            "Контроллер области Api наследует ApiControllerBase — иначе у него нет проверки "
            + $"антифоржери и ProblemDetails. Нарушители: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Api_controller_base_should_validate_antiforgery_and_behave_as_api()
    {
        var baseType = typeof(ApiControllerBase);

        baseType.IsDefined(typeof(AutoValidateAntiforgeryTokenAttribute), inherit: true)
            .ShouldBeTrue("ApiControllerBase обязан проверять антифоржери-токен на изменяющих запросах.");
        baseType.IsDefined(typeof(ApiControllerAttribute), inherit: true)
            .ShouldBeTrue("ApiControllerBase обязан быть [ApiController]: ошибки модели — ValidationProblemDetails.");
        baseType.GetCustomAttributes(typeof(AreaAttribute), inherit: true)
            .Cast<AreaAttribute>()
            .ShouldContain(area => area.RouteValue == ApiControllerBase.AreaName);
    }

    [Fact]
    public void Api_area_should_contain_controllers()
    {
        ApiControllers().ShouldNotBeEmpty();
    }

    private static Type[] ApiControllers() =>
        [.. ArchitectureFixture.Web.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(ControllerBase).IsAssignableFrom(type)
                && type.Namespace?.StartsWith(ApiNamespace, StringComparison.Ordinal) == true)];
}