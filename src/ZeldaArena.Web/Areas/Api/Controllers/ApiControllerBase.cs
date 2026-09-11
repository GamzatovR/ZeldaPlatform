using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Domain.Common;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>
/// Основа всех эндпоинтов <c>Areas/Api</c> (docs/SPEC.md §10.1). Здесь только транспорт:
/// сценарии те же, что у страниц, и API-контроллер, как и обычный, лишь отправляет
/// запрос и выбирает форму ответа (§5.2, правило 4).
///
/// Для списков ответ — partial HTML, тот же файл, что рисует страница; для операций —
/// JSON. Отказ сценария — <see cref="ProblemDetails"/> с переведённым текстом и кодом
/// ошибки, чтобы клиент мог и показать сообщение, и отличить один отказ от другого.
///
/// Антифоржери проверяется на каждом изменяющем запросе автоматически (§15): атрибут
/// висит здесь, и архитектурный тест требует, чтобы каждый контроллер области
/// наследовал этот класс, — забыть его на новом эндпоинте нельзя.
/// </summary>
[Area(AreaName)]
[ApiController]
[AutoValidateAntiforgeryToken]
[TypeFilter<ApiExceptionFilter>]
public abstract class ApiControllerBase(IStringLocalizer<SharedResource> localizer) : Controller
{
    public const string AreaName = "Api";

    /// <summary>Все адреса области начинаются отсюда; по нему же cookie-аутентификация
    /// отвечает кодом, а не редиректом на страницу входа.</summary>
    public const string PathPrefix = "/api";

    protected IStringLocalizer<SharedResource> Localizer => localizer;

    /// <summary>
    /// Partial списка. <paramref name="pagePath"/> — адрес страницы, которой принадлежит
    /// список: на него ведут ссылки пагинации (<see cref="ListViewData.PagePath"/>).
    /// </summary>
    protected PartialViewResult ListPartial(string viewName, object model, string? pagePath)
    {
        ViewData[ListViewData.PagePath] = pagePath;

        return PartialView(viewName, model);
    }

    /// <summary>Отказ сценария: текст на языке пользователя плюс код для клиента.</summary>
    protected ObjectResult Failure(Error error, int statusCode = StatusCodes.Status400BadRequest)
    {
        ArgumentNullException.ThrowIfNull(error);

        return Failure(localizer.ForError(error), error.Code, statusCode);
    }

    protected ObjectResult Failure(string detail, string? code, int statusCode = StatusCodes.Status400BadRequest)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode, detail: detail);

        if (code is not null)
        {
            problem.Extensions["code"] = code;
        }

        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
