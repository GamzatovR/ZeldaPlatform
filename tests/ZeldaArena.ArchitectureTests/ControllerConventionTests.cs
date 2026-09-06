using Microsoft.AspNetCore.Mvc;

namespace ZeldaArena.ArchitectureTests;

/// <summary>
/// Правило 4 из docs/SPEC.md §5.2: контроллер — тонкий слой, он умеет только отправить
/// команду или запрос, перевести текст и написать в лог. Всё остальное — признак того,
/// что бизнес-логика поехала в слой представления (§20, пункт 1).
/// </summary>
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
        var offenders = ArchitectureFixture.Web.GetTypes()
            .Where(IsController)
            .SelectMany(controller => controller.GetConstructors())
            .SelectMany(constructor => constructor.GetParameters()
                .Where(parameter => !IsAllowed(parameter.ParameterType))
                .Select(parameter =>
                    $"{constructor.DeclaringType!.Name}({parameter.ParameterType.Name} {parameter.Name})"))
            .ToArray();

        offenders.ShouldBeEmpty(
            "Контроллер принимает только ISender, IStringLocalizer и ILogger. "
            + $"Нарушители: {string.Join(", ", offenders)}");
    }

    private static bool IsController(Type type) =>
        typeof(ControllerBase).IsAssignableFrom(type)
        && type is { IsAbstract: false, IsClass: true }
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