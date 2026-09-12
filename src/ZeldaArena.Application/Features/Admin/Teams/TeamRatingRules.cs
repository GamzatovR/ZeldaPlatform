using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams;

public static class TeamRatingRules
{
    public const int MaxRating = 10_000;

    public static IRuleBuilderOptions<T, int> ValidRating<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(0, MaxRating)
            .WithMessage($"Рейтинг — от 0 до {MaxRating}.");
}