using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeams;

/// <summary>
/// Как у турниров: настоящие ошибки отвергаются, размер страницы и сортировка
/// нормализуются молча (§10.2).
/// </summary>
public sealed class GetTeamsQueryValidator : AbstractValidator<GetTeamsQuery>
{
    public const int MaxSearchLength = 100;

    public GetTeamsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"Строка поиска не длиннее {MaxSearchLength} символов.");

        RuleFor(query => query.RatingMin)
            .GreaterThanOrEqualTo(0)
            .When(query => query.RatingMin.HasValue)
            .WithMessage("Рейтинг не может быть отрицательным.");

        RuleFor(query => query.Region)
            .IsInEnum()
            .When(query => query.Region.HasValue)
            .WithMessage("Неизвестный регион.");
    }
}