using FluentValidation;

namespace ZeldaArena.Application.Features.News.Queries.GetLatestNews;

/// <summary>
/// Ограничение размера ленты — та же роль, что и у валидатора витрины матчей:
/// каждый сценарий получает валидатор, даже когда параметр приходит из кода
/// (CLAUDE.md, «Архитектурные правила»).
/// </summary>
public sealed class GetLatestNewsQueryValidator : AbstractValidator<GetLatestNewsQuery>
{
    public const int MaxCount = 12;

    public GetLatestNewsQueryValidator()
    {
        RuleFor(query => query.Count)
            .InclusiveBetween(1, MaxCount);
    }
}