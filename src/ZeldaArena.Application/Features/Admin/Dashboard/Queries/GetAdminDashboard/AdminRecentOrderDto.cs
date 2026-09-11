using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

public sealed record AdminRecentOrderDto(
    string Number,
    DateTimeOffset PlacedAt,
    OrderStatus Status,
    decimal Total,
    string Currency);