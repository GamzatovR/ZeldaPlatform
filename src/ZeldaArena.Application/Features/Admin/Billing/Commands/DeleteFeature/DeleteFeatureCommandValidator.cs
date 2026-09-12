using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.DeleteFeature;

public sealed class DeleteFeatureCommandValidator : AbstractValidator<DeleteFeatureCommand>
{
    public DeleteFeatureCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указана функция.");
    }
}