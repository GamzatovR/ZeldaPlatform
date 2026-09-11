using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Exceptions;

namespace ZeldaArena.Web.Areas.Api;

/// <summary>
/// Исключения сценариев, у которых есть честный ответ клиенту, превращаются
/// в <see cref="ProblemDetails"/> (docs/SPEC.md §14.2: «в Areas/Api вместо HTML —
/// ProblemDetails»):
///
/// <list type="bullet">
/// <item>отказ FluentValidation — 400 с ошибками по полям. Испорченная ссылка вроде
/// <c>?page=0</c> — ошибка клиента, а не сервера;</item>
/// <item>конфликт параллельных изменений — 409 с просьбой повторить.</item>
/// </list>
///
/// Остальное фильтр не трогает: неожиданное исключение — ошибка сервера, и притворяться
/// иначе нельзя. Глобальная обработка для HTML-страниц — <c>GlobalExceptionHandlingMiddleware</c>
/// Фазы 11; этот фильтр действует только внутри области Api.
/// </summary>
public sealed class ApiExceptionFilter(
    ProblemDetailsFactory problemDetailsFactory,
    IStringLocalizer<SharedResource> localizer)
    : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        switch (context.Exception)
        {
            case ValidationException validation:
                var modelState = new ModelStateDictionary();
                foreach (var failure in validation.Errors)
                {
                    modelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
                }

                context.Result = Respond(problemDetailsFactory.CreateValidationProblemDetails(
                    context.HttpContext, modelState, StatusCodes.Status400BadRequest));
                context.ExceptionHandled = true;
                break;

            case ConcurrencyConflictException:
                context.Result = Respond(problemDetailsFactory.CreateProblemDetails(
                    context.HttpContext,
                    StatusCodes.Status409Conflict,
                    detail: localizer["api.concurrent_change"].Value));
                context.ExceptionHandled = true;
                break;
        }
    }

    private static ObjectResult Respond(ProblemDetails problem) =>
        new(problem) { StatusCode = problem.Status };
}
