using FluentValidation;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;

public sealed class GetTournamentsQueryValidator : AbstractValidator<GetTournamentsQuery>
{
    public const int MaxSearchLength = 100;

    public GetTournamentsQueryValidator()
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

        RuleFor(query => query.PrizeMin)
            .GreaterThanOrEqualTo(0)
            .When(query => query.PrizeMin.HasValue)
            .WithMessage("Призовой фонд не может быть отрицательным.");

        RuleFor(query => query.Region)
            .IsInEnum()
            .When(query => query.Region.HasValue)
            .WithMessage("Неизвестный регион.");
    }
}