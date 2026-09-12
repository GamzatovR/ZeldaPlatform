using FluentValidation;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.CreatePlayer;

public sealed class CreatePlayerCommandValidator : AbstractValidator<CreatePlayerCommand>
{
    public CreatePlayerCommandValidator(IDateTimeProvider clock) => Include(new PlayerFieldsValidator(clock));
}