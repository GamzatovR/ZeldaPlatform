using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.SetPlanFeatures;

public sealed class SetPlanFeaturesCommandValidator : AbstractValidator<SetPlanFeaturesCommand>
{
    /// <summary>Столько же отведено под значение в базе (docs/SPEC.md §6).</summary>
    public const int MaxValueLength = 200;

    public SetPlanFeaturesCommandValidator()
    {
        RuleFor(command => command.PlanId)
            .NotEmpty()
            .WithMessage("Тариф не указан.");

        RuleFor(command => command.Features)
            .NotNull()
            .WithMessage("Набор фич не передан.");

        RuleForEach(command => command.Features).ChildRules(feature =>
        {
            feature.RuleFor(assignment => assignment.FeatureId)
                .NotEmpty()
                .WithMessage("Фича не указана.");

            feature.RuleFor(assignment => assignment.Value)
                .MaximumLength(MaxValueLength)
                .WithMessage($"Значение параметра не длиннее {MaxValueLength} символов.");
        });

        // Одна фича дважды в наборе — это спор о значении параметра, который
        // нечем разрешить. Дубли отвергаются, а не схлопываются молча.
        RuleFor(command => command.Features)
            .Must(features => features is null
                || features.Select(feature => feature.FeatureId).Distinct().Count() == features.Count)
            .WithMessage("Одна и та же фича указана дважды.");
    }
}