using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;

public sealed class GetMatchesForAdminQueryValidator : AbstractValidator<GetMatchesForAdminQuery>
{
    public GetMatchesForAdminQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue)
            .WithMessage("Неизвестный статус матча.");
    }
}