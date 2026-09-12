using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;

public sealed class GetTournamentsForAdminQueryValidator : AbstractValidator<GetTournamentsForAdminQuery>
{
    public const int MaxSearchLength = 100;

    public GetTournamentsForAdminQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"Строка поиска не длиннее {MaxSearchLength} символов.");

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue)
            .WithMessage("Неизвестный статус турнира.");

        RuleFor(query => query.Region)
            .IsInEnum()
            .When(query => query.Region.HasValue)
            .WithMessage("Неизвестный регион.");
    }
}