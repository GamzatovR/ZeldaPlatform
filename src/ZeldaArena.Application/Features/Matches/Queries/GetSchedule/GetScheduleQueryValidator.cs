using FluentValidation;

namespace ZeldaArena.Application.Features.Matches.Queries.GetSchedule;

public sealed class GetScheduleQueryValidator : AbstractValidator<GetScheduleQuery>
{
    public GetScheduleQueryValidator()
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