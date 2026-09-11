using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

/// <summary>
/// Дашборд. Спортивная часть видна и модератору, деловая — только администратору:
/// у модератора нет доступа к пользователям, подпискам и платежам (docs/SPEC.md §8.1).
/// </summary>
public sealed record AdminDashboardDto
{
    public int LiveMatchCount { get; init; }

    /// <summary>Команды подписчиков, ждущие одобрения, — очередь работы модератора.</summary>
    public int TeamsAwaitingApproval { get; init; }

    /// <summary>Идущие матчи впереди ближайших запланированных.</summary>
    public IReadOnlyList<MatchCardDto> UpcomingMatches { get; init; } = [];

    /// <summary>
    /// <see langword="null"/>, если смотрит не администратор: данных нет в ответе вовсе,
    /// а не просто в разметке — спрятать блок во вьюхе и забыть проверку было бы нечем.
    /// </summary>
    public AdminBusinessFiguresDto? Business { get; init; }
}