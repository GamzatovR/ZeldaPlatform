using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.DeletePlayer;

public sealed class DeletePlayerCommandValidator : AbstractValidator<DeletePlayerCommand>
{
    public DeletePlayerCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан игрок.");
    }
}