using FluentValidation;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

public sealed class GetUsersForAdminQueryValidator : AbstractValidator<GetUsersForAdminQuery>
{
    public const int MaxSearchLength = 100;

    public GetUsersForAdminQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"Строка поиска не длиннее {MaxSearchLength} символов.");

        RuleFor(query => query.Role)
            .Must(role => role is null || RoleNames.All.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Неизвестная роль.");
    }
}