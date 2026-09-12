using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams;

/// <summary>
/// Рейтинг команды правит только администрация: на сайте он показан, но не меняется.
/// Нижнюю границу держит инвариант <c>Team.UpdateRating</c>, верхняя — предел формы:
/// домену она безразлична, а поле ввода без предела принимает что угодно.
/// </summary>
public static class TeamRatingRules
{
    public const int MaxRating = 10_000;

    public static IRuleBuilderOptions<T, int> ValidRating<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(0, MaxRating)
            .WithMessage($"Рейтинг — от 0 до {MaxRating}.");
}