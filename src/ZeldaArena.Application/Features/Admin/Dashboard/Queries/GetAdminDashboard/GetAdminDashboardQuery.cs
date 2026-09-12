using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

public sealed record GetAdminDashboardQuery(int MatchCount = 5, int OrderCount = 5)
    : IQuery<AdminDashboardDto>;