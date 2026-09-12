using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Domain.Common;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

/// <summary>
/// Основа страниц админки — Area <c>Admin</c> (docs/SPEC.md §9.4).
///
/// Доступ здесь не объявлен намеренно: его на всю область вешает
/// <see cref="AdminAreaConvention"/> — одно место вместо атрибута на каждом контроллере,
/// который можно забыть (§8.1: «доступ в Area Admin — через соглашение на всю область»).
/// Архитектурный тест требует, чтобы каждый контроллер области наследовал этот класс,
/// поэтому мимо соглашения новый контроллер не пройдёт.
///
/// Антифоржери проверяется на каждом изменяющем запросе автоматически (§15).
/// </summary>
[Area(AreaName)]
[AutoValidateAntiforgeryToken]
public abstract class AdminControllerBase : Controller
{
    public const string AreaName = "Admin";

    /// <summary>Сообщение об успехе, пережившее редирект (PRG).</summary>
    protected void ReportSuccess(string message) =>
        TempData[TempDataKeys.StatusMessage] = message;

    /// <summary>Сообщение об отказе: ведущий «!» — признак ошибки для <c>_StatusMessage</c>.</summary>
    protected void ReportFailure(string message) =>
        TempData[TempDataKeys.StatusMessage] = "!" + message;

    /// <summary>
    /// Исход действия в строке или на карточке: успех — заданным текстом, отказ —
    /// переведённым кодом ошибки сценария (<see cref="ErrorLocalizationExtensions.ForError"/>).
    /// </summary>
    protected void Report(Result result, string successMessage, IStringLocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(localizer);

        if (result.IsSuccess)
        {
            ReportSuccess(successMessage);
        }
        else
        {
            ReportFailure(localizer.ForError(result.Error));
        }
    }

    /// <summary>
    /// Первая ошибка привязки маленькой формы в строке таблицы. Такая форма не
    /// перерисовывается с подсветкой полей — её отказ уходит сообщением на страницу.
    /// </summary>
    protected string FirstModelError() =>
        ModelState.Values.SelectMany(entry => entry.Errors).Select(error => error.ErrorMessage)
            .FirstOrDefault(message => !string.IsNullOrEmpty(message)) ?? string.Empty;
}