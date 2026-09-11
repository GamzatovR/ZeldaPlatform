using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Web.Constants;

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
}