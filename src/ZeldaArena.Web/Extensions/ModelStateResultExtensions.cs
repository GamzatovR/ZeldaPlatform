using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Web.Extensions;

/// <summary>
/// Перенос неудачного <see cref="Result"/> в <see cref="ModelStateDictionary"/>.
///
/// Хендлеры возвращают <see cref="Error"/> с ключом ресурса, а не с готовой фразой
/// (docs/SPEC.md §9.5), поэтому текст подставляется здесь — на слое представления,
/// где уже есть текущая культура.
///
/// Ошибка кладётся в общую сводку, а не на конкретное поле: «неверный адрес или
/// пароль» не относится ни к одному из них по отдельности, и подсветка только поля
/// пароля подсказала бы, что адрес угадан верно.
/// </summary>
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

        // Отсутствующий ключ ResourceManager возвращает как есть, поэтому незакрытый
        // перевод даст код ошибки вместо пустой строки — это заметно при вычитке.
        modelState.AddModelError(string.Empty, localizer[result.Error.Code]);
    }
}