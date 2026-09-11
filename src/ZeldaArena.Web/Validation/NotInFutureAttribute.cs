using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Validation;

/// <summary>
/// Дата не позже сегодняшней (по UTC) — пара к правилу валидатора сценария
/// «дата основания не может быть в будущем».
///
/// Без неё будущая дата, выбранная обычной формой, проходила бы проверку модели
/// и отвергалась только FluentValidation внутри конвейера, а его отказ до Фазы 11
/// отвечает ошибкой сервера. Здесь отказ ложится в ModelState и возвращается
/// на форму сообщением (docs/SPEC.md §15). В браузере то же ограничение даёт
/// атрибут <c>max</c> у поля даты — и работает без JavaScript.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute()
        : base("Дата не может быть в будущем.")
    {
    }

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public override bool IsValid(object? value) =>
        value is not DateOnly date || date <= Today;
}