using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;

public sealed class GetTeamsForAdminQueryValidator : AbstractValidator<GetTeamsForAdminQuery>
{
    public const int MaxSearchLength = 100;

    public GetTeamsForAdminQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"Строка поиска не длиннее {MaxSearchLength} символов.");

        RuleFor(query => query.Region)
            .IsInEnum()
            .When(query => query.Region.HasValue)
            .WithMessage("Неизвестный регион.");
    }
}