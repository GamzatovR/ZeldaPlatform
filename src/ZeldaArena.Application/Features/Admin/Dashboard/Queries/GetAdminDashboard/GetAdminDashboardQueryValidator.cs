using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

/// <summary>
/// Размеры блоков приходят из кода, а не от пользователя, но соглашение
/// «запрос + хендлер + валидатор» действует без исключений (docs/CONVENTIONS.md).
/// </summary>
public sealed class GetAdminDashboardQueryValidator : AbstractValidator<GetAdminDashboardQuery>
{
    public const int MaxCount = 20;

    public GetAdminDashboardQueryValidator()
    {
        RuleFor(query => query.MatchCount).InclusiveBetween(1, MaxCount);
        RuleFor(query => query.OrderCount).InclusiveBetween(1, MaxCount);
    }
}