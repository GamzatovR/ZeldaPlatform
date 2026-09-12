using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;

public sealed class GetMatchConsoleQueryValidator : AbstractValidator<GetMatchConsoleQuery>
{
    public GetMatchConsoleQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty().WithMessage("Не указан матч.");
    }
}