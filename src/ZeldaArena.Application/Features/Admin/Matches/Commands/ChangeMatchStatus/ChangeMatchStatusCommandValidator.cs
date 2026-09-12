using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.ChangeMatchStatus;

public sealed class ChangeMatchStatusCommandValidator : AbstractValidator<ChangeMatchStatusCommand>
{
    public ChangeMatchStatusCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан матч.");
        RuleFor(command => command.Transition).IsInEnum().WithMessage("Неизвестное действие с матчем.");
    }
}