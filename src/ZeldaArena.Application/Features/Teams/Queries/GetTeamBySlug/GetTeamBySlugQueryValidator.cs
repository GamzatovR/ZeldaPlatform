using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;

/// <summary>Неразборчивый слаг — «команды нет» и 404, а не ошибка валидации.</summary>
public sealed class GetTeamBySlugQueryValidator : AbstractValidator<GetTeamBySlugQuery>
{
    public GetTeamBySlugQueryValidator()
    {
        RuleFor(query => query.Slug).NotEmpty();
    }
}