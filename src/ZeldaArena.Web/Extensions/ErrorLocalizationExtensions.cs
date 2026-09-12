using Microsoft.Extensions.Localization;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Web.Extensions;

public static class ErrorLocalizationExtensions
{
    public static string ForError(this IStringLocalizer localizer, Error error)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(error);

        // Отсутствующий ключ ResourceManager возвращает как есть, поэтому незакрытый
        // перевод даст код ошибки вместо пустой строки — это заметно при вычитке.
        return error.Arguments.Count == 0
            ? localizer[error.Code].Value
            : localizer[error.Code, [.. error.Arguments]].Value;
    }
}