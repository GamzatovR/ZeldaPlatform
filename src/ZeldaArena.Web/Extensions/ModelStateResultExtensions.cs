using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Web.Extensions;

public static class ModelStateResultExtensions
{
    public static void AddResultError(
        this ModelStateDictionary modelState,
        Result result,
        IStringLocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(localizer);

        if (result.IsSuccess)
        {
            return;
        }

        modelState.AddModelError(string.Empty, localizer.ForError(result.Error));
    }
}