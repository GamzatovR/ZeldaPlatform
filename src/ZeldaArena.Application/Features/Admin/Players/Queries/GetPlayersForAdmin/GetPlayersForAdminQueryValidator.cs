using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayersForAdmin;

public sealed class GetPlayersForAdminQueryValidator : AbstractValidator<GetPlayersForAdminQuery>
{
    public const int MaxSearchLength = 100;

    public GetPlayersForAdminQueryValidator()
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