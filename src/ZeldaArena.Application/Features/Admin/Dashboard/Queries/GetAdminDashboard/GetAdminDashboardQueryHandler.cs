using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

/// <summary>
/// Собирает дашборд. Каждый показатель — один скалярный запрос, считает база: ни одна
/// коллекция ради числа в память не поднимается (урок Фазы 2, docs/PROGRESS.md).
///
/// Деловая часть собирается, только если смотрит администратор. Проверка здесь, а не во
/// вьюхе: модератору эти данные не должны уходить вовсе (docs/SPEC.md §8.1), и тот же
/// сценарий вызовет мобильный клиент (EP-2), у которого вьюхи нет.
/// </summary>
public sealed class GetAdminDashboardQueryHandler(
    IReadRepository<Match> matches,
    IReadRepository<Team> teams,
    IReadRepository<Subscription> subscriptions,
    IReadRepository<Payment> payments,
    IReadRepository<Order> orders,
    IUserAdministrationService users,
    IQueryExecutor queryExecutor,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private static readonly TimeSpan RevenueWindow = TimeSpan.FromDays(30);

    public async Task<AdminDashboardDto> Handle(
        GetAdminDashboardQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var liveMatchCount = await queryExecutor.CountAsync(
            matches.Query().Where(match => match.Status == MatchStatus.Live),
            cancellationToken);

        var teamsAwaitingApproval = await queryExecutor.CountAsync(
            teams.Query().Where(team => !team.IsApproved),
            cancellationToken);

        // Та же выборка и тот же порядок, что на главной: идущие впереди запланированных.
        var upcoming = await queryExecutor.ToListAsync(
            matches.Query()
                .Where(match => match.Status == MatchStatus.Live || match.Status == MatchStatus.Scheduled)
                .OrderBy(match => match.Status == MatchStatus.Live ? 0 : 1)
                .ThenBy(match => match.ScheduledAt)
                .Take(request.MatchCount)
                .Select(MatchCardProjection.Expression),
            cancellationToken);

        return new AdminDashboardDto
        {
            LiveMatchCount = liveMatchCount,
            TeamsAwaitingApproval = teamsAwaitingApproval,
            UpcomingMatches = upcoming,
            Business = currentUser.IsInRole(RoleNames.Admin)
                ? await GetBusinessFiguresAsync(request.OrderCount, cancellationToken)
                : null,
        };
    }

    private async Task<AdminBusinessFiguresDto> GetBusinessFiguresAsync(
        int orderCount,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var windowStart = now - RevenueWindow;

        var userCount = await users.CountUsersAsync(cancellationToken);

        var activeSubscriptions = await queryExecutor.CountAsync(
            subscriptions.Query().Where(subscription =>
                subscription.Status == SubscriptionStatus.Active && subscription.EndsAt > now),
            cancellationToken);

        // Все цены проекта в одной валюте; фильтр по ней — страховка от того, чтобы
        // однажды сложить рубли с чем-то ещё (Money не смешивает валюты, SQL — смешает).
        var succeeded = payments.Query().Where(payment =>
            payment.Status == PaymentStatus.Succeeded
            && payment.Amount.Currency == Money.DefaultCurrency);

        var revenueTotal = await queryExecutor.SumAsync(
            succeeded.Select(payment => payment.Amount.Amount),
            cancellationToken);

        var revenueLast30Days = await queryExecutor.SumAsync(
            succeeded
                .Where(payment => payment.PaidAt >= windowStart)
                .Select(payment => payment.Amount.Amount),
            cancellationToken);

        var ordersToShip = await queryExecutor.CountAsync(
            orders.Query().Where(order => order.Status == OrderStatus.Paid),
            cancellationToken);

        var recentOrders = await queryExecutor.ToListAsync(
            orders.Query()
                .OrderByDescending(order => order.PlacedAt)
                .Take(orderCount)
                .Select(order => new AdminRecentOrderDto(
                    order.Number,
                    order.PlacedAt,
                    order.Status,
                    order.Total,
                    order.Currency)),
            cancellationToken);

        return new AdminBusinessFiguresDto
        {
            UserCount = userCount,
            ActiveSubscriptions = activeSubscriptions,
            RevenueTotal = revenueTotal,
            RevenueLast30Days = revenueLast30Days,
            Currency = Money.DefaultCurrency,
            OrdersToShip = ordersToShip,
            RecentOrders = recentOrders,
        };
    }
}