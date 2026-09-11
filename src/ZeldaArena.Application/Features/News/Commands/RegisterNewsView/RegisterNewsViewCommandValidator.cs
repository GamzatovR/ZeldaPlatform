using FluentValidation;

namespace ZeldaArena.Application.Features.News.Commands.RegisterNewsView;

public sealed class RegisterNewsViewCommandValidator : AbstractValidator<RegisterNewsViewCommand>
{
    public RegisterNewsViewCommandValidator()
    {
        RuleFor(command => command.ArticleId).NotEmpty();
    }
}