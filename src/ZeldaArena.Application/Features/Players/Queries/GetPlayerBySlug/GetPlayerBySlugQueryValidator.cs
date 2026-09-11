using FluentValidation;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;

/// <summary>Неразборчивый слаг — «игрока нет» и 404, а не ошибка валидации.</summary>
public sealed class GetPlayerBySlugQueryValidator : AbstractValidator<GetPlayerBySlugQuery>
{
    public GetPlayerBySlugQueryValidator()
    {
        RuleFor(query => query.Slug).NotEmpty();
    }
}