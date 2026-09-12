using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

public sealed record AdminDashboardDto
{
    public int LiveMatchCount { get; init; }

    /// <summary>Команды подписчиков, ждущие одобрения, — очередь работы модератора.</summary>
    public int TeamsAwaitingApproval { get; init; }

    /// <summary>Идущие матчи впереди ближайших запланированных.</summary>
    public IReadOnlyList<MatchCardDto> UpcomingMatches { get; init; } = [];

    public AdminBusinessFiguresDto? Business { get; init; }
}