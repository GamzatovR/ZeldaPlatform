using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.ToggleFeature;

public sealed class ToggleFeatureCommandValidator : AbstractValidator<ToggleFeatureCommand>
{
    public ToggleFeatureCommandValidator() =>
        RuleFor(command => command.FeatureId)
            .NotEmpty()
            .WithMessage("Функция не указана.");
}