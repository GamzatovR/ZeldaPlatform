using FluentValidation;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayers;

public sealed class GetPlayersQueryValidator : AbstractValidator<GetPlayersQuery>
{
    public const int MaxSearchLength = 100;

    public GetPlayersQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"Строка поиска не длиннее {MaxSearchLength} символов.");

        RuleFor(query => query.Role)
            .IsInEnum()
            .When(query => query.Role.HasValue)
            .WithMessage("Неизвестная роль игрока.");
    }
}