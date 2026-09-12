using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

public sealed class GetAdminDashboardQueryValidator : AbstractValidator<GetAdminDashboardQuery>
{
    public const int MaxCount = 20;

    public GetAdminDashboardQueryValidator()
    {
        RuleFor(query => query.MatchCount).InclusiveBetween(1, MaxCount);
        RuleFor(query => query.OrderCount).InclusiveBetween(1, MaxCount);
    }
}