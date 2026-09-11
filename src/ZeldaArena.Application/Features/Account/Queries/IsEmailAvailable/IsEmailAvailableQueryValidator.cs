using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;

/// <summary>
/// Только границы ввода. Формат адреса здесь не проверяется намеренно: за него
/// отвечает своё правило формы, а Remote-проверка отвечает на один вопрос —
/// занят ли адрес. Отказ на неверном формате превратил бы её ответ в ошибку запроса,
/// и поле зависло бы в ожидании.
/// </summary>
public sealed class IsEmailAvailableQueryValidator : AbstractValidator<IsEmailAvailableQuery>
{
    public IsEmailAvailableQueryValidator()
    {
        RuleFor(query => query.Email)
            .NotEmpty()
            .WithMessage("Укажите адрес электронной почты.")
            .MaximumLength(AccountValidationRules.MaxEmailLength)
            .WithMessage($"Адрес не длиннее {AccountValidationRules.MaxEmailLength} символов.");
    }
}
