using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Domain.Common;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Api.Controllers;

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

    protected PartialViewResult ListPartial(string viewName, object model, string? pagePath)
    {
        ViewData[ListViewData.PagePath] = pagePath;

        return PartialView(viewName, model);
    }

    protected string PageUrl(string action, string controller, object? values = null)
    {
        var routeValues = new RouteValueDictionary(values) { ["area"] = string.Empty };

        return Url.Action(action, controller, routeValues)
            ?? throw new InvalidOperationException($"Нет маршрута к {controller}.{action}.");
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