using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing;

public static class PlanFieldsValidator
{
    public const int MaxCodeLength = 64;
    public const int MaxNameLength = 128;
    public const int MaxDescriptionLength = 512;
    public const decimal MaxPrice = 1_000_000m;
    public const int MaxDurationDays = 3650;

    public const string CodePattern = "^[a-z][a-z0-9]*(-[a-z0-9]+)*$";

    public static IRuleBuilderOptions<T, string> ValidPlanCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .WithMessage("Укажите код тарифа.")
            .MaximumLength(MaxCodeLength)
            .WithMessage($"Код не длиннее {MaxCodeLength} символов.")
            .Matches(CodePattern)
            .WithMessage("Код — латиница, цифры и дефисы, например pro-month.");

    public static IRuleBuilderOptions<T, string> ValidPlanName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .WithMessage("Укажите название тарифа.")
            .MaximumLength(MaxNameLength)
            .WithMessage($"Название не длиннее {MaxNameLength} символов.");

    public static IRuleBuilderOptions<T, string?> ValidPlanDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(MaxDescriptionLength)
            .WithMessage($"Описание не длиннее {MaxDescriptionLength} символов.");

    public static IRuleBuilderOptions<T, decimal> ValidPlanPrice<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.InclusiveBetween(0m, MaxPrice)
            .WithMessage($"Цена — от 0 до {MaxPrice}.");

    public static IRuleBuilderOptions<T, int> ValidPlanDuration<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(0, MaxDurationDays)
            .WithMessage($"Срок — от 0 до {MaxDurationDays} дней.");
}