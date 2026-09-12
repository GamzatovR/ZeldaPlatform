using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;

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