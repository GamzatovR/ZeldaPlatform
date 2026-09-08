using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.RegisterUser;

/// <summary>
/// Серверная половина двухуровневой валидации (docs/SPEC.md §15). Работает всегда,
/// в том числе при выключенном JavaScript, — это отдельный пункт чек-листа §19.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>Длина отображаемого имени: столько же отведено в базе.</summary>
    public const int MaxDisplayNameLength = 64;

    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Email).ValidAccountEmail();

        RuleFor(command => command.Password).ValidPassword();

        RuleFor(command => command.DisplayName)
            .MaximumLength(MaxDisplayNameLength)
            .WithMessage($"Отображаемое имя не длиннее {MaxDisplayNameLength} символов.");

        RuleFor(command => command.PreferredCulture).SupportedCulture();
    }
}