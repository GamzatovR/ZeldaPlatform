using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Exceptions;

namespace ZeldaArena.Web.Areas.Api;

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