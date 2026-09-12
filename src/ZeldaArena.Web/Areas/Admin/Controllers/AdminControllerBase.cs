using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Domain.Common;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

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

    protected string FirstModelError() =>
        ModelState.Values.SelectMany(entry => entry.Errors).Select(error => error.ErrorMessage)
            .FirstOrDefault(message => !string.IsNullOrEmpty(message)) ?? string.Empty;
}