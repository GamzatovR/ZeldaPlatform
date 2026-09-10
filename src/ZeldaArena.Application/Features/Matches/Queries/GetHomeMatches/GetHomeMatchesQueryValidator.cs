using FluentValidation;

namespace ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;

/// <summary>
/// Ограничение размера витрины. Значение приходит из кода, а не от пользователя,
/// но запрос без валидатора нарушил бы соглашение «сценарий = команда/запрос +
/// хендлер + валидатор» (CLAUDE.md) и не был бы прикрыт от опечатки в вызове.
/// </summary>
public sealed class GetHomeMatchesQueryValidator : AbstractValidator<GetHomeMatchesQuery>
{
    public const int MaxCount = 12;

    public GetHomeMatchesQueryValidator()
    {
        RuleFor(query => query.Count)
            .InclusiveBetween(1, MaxCount);
    }
}