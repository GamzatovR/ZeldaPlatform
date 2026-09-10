using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.ArchitectureTests;

/// <summary>
/// Правило 4 из docs/SPEC.md §5.2: контроллер — тонкий слой, он умеет только отправить
/// команду или запрос, перевести текст и написать в лог. Всё остальное — признак того,
/// что бизнес-логика поехала в слой представления (§20, пункт 1).
///
/// Проверка распространена и на <see cref="PageModel"/>: страницы аккаунта из Фазы 3 —
/// это Razor Pages, и без этого соглашение на них попросту не действовало бы.
/// Формулировка §5.2 говорит про «контроллеры», но имеет в виду весь слой
/// представления: PageModel — такой же обработчик запроса.
///
/// С Фазы 5 сюда же попали ViewComponent: шапка и подвал собирают данные ровно так же,
/// как контроллер, и лазейка «взять сервис в компонент, раз в контроллер нельзя»
/// закрыта до того, как ей успели воспользоваться. Компоненты общих блоков из §10.2
/// (мини-корзина, колокольчик, лента новостей) появятся в Фазах 7 и 10 — правило
/// должно действовать на них с первого дня.
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

    /// <summary>
    /// Наследование от <see cref="ViewComponent"/> не обязательно: компонентом
    /// считается и класс с суффиксом в имени, и помеченный атрибутом. Проверяются
    /// все три способа, иначе правило обходится сменой базового класса.
    /// </summary>
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