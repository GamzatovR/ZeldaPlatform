using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.DeleteMatch;

public sealed class DeleteMatchCommandValidator : AbstractValidator<DeleteMatchCommand>
{
    public DeleteMatchCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан матч.");
    }
}