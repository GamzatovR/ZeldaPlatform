using FluentValidation;

using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Application.Features.Account.Commands.RegisterUser;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Account.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.DisplayName)
            .MaximumLength(RegisterUserCommandValidator.MaxDisplayNameLength)
            .WithMessage(
                $"Отображаемое имя не длиннее {RegisterUserCommandValidator.MaxDisplayNameLength} символов.");

        // Страна хранится кодом ISO 3166-1 alpha-2 — тем же, что доменный CountryCode.
        RuleFor(command => command.CountryCode)
            .Must(code => string.IsNullOrEmpty(code) || CountryCode.TryFrom(code, out _))
            .WithMessage("Код страны состоит из двух латинских букв, например RU.");

        RuleFor(command => command.PreferredCulture).SupportedCulture();
    }
}