using FluentValidation;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

/// <summary>
/// Только наличие. Слаг, из которого ничего не складывается, — это «турнира нет»
/// и ответ 404, а не ошибка валидации: ссылку могли перепечатать с опечаткой.
/// Длину и допустимые символы проверяет <c>Slug.TryFrom</c> в хендлере.
/// </summary>
public sealed class GetTournamentBySlugQueryValidator : AbstractValidator<GetTournamentBySlugQuery>
{
    public GetTournamentBySlugQueryValidator()
    {
        RuleFor(query => query.Slug).NotEmpty();
    }
}