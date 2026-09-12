using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.ArchitectureTests;

public class ControllerConventionTests
{
    private static readonly string[] AllowedDependencies =
    [
        "MediatR.ISender",
        "Microsoft.Extensions.Localization.IStringLocalizer",
        "Microsoft.Extensions.Localization.IStringLocalizer`1",
        "Microsoft.Extensions.Logging.ILogger",
        "Microsoft.Extensions.Logging.ILogger`1"
    ];

    [Fact]
    public void Controllers_should_only_inject_sender_localizer_and_logger()
    {
        var offenders = Offenders(IsController);

        offenders.ShouldBeEmpty(
            "Контроллер принимает только ISender, IStringLocalizer и ILogger. "
            + $"Нарушители: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Page_models_should_only_inject_sender_localizer_and_logger()
    {
        var offenders = Offenders(IsPageModel);

        offenders.ShouldBeEmpty(
            "PageModel принимает только ISender, IStringLocalizer и ILogger. "
            + $"Нарушители: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void View_components_should_only_inject_sender_localizer_and_logger()
    {
        var offenders = Offenders(IsViewComponent);

        offenders.ShouldBeEmpty(
            "ViewComponent принимает только ISender, IStringLocalizer и ILogger. "
            + $"Нарушители: {string.Join(", ", offenders)}");
    }

    private static string[] Offenders(Func<Type, bool> selector) =>
        [.. ArchitectureFixture.Web.GetTypes()
            .Where(selector)
            .SelectMany(handler => handler.GetConstructors())
            .SelectMany(constructor => constructor.GetParameters()
                .Where(parameter => !IsAllowed(parameter.ParameterType))
                .Select(parameter =>
                    $"{constructor.DeclaringType!.Name}({parameter.ParameterType.Name} {parameter.Name})"))];

    private static bool IsController(Type type) =>
        typeof(ControllerBase).IsAssignableFrom(type) && IsConcreteHandler(type);

    private static bool IsPageModel(Type type) =>
        typeof(PageModel).IsAssignableFrom(type) && IsConcreteHandler(type);

    private static bool IsViewComponent(Type type) =>
        IsConcreteHandler(type)
        && (typeof(ViewComponent).IsAssignableFrom(type)
            || type.IsDefined(typeof(ViewComponentAttribute), inherit: true)
            || type.Name.EndsWith("ViewComponent", StringComparison.Ordinal));

    private static bool IsConcreteHandler(Type type) =>
        type is { IsAbstract: false, IsClass: true }
        && !ArchitectureFixture.IsCompilerGenerated(type);

    private static bool IsAllowed(Type parameterType)
    {
        var definition = parameterType.IsGenericType
            ? parameterType.GetGenericTypeDefinition()
            : parameterType;

        var fullName = definition.FullName ?? definition.Name;

        return AllowedDependencies.Contains(fullName, StringComparer.Ordinal);
    }
}