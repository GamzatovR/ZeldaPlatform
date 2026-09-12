using FluentValidation;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.UpdatePlayer;

public sealed class UpdatePlayerCommandValidator : AbstractValidator<UpdatePlayerCommand>
{
    public UpdatePlayerCommandValidator(IDateTimeProvider clock)
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан игрок.");

        Include(new PlayerFieldsValidator(clock));
    }
}