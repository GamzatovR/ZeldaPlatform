using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;

/// <summary>Одна запись истории составов: где, в какой роли и когда.</summary>
public sealed record PlayerTeamHistoryDto
{
    public string TeamSlug { get; init; } = string.Empty;

    public string TeamName { get; init; } = string.Empty;

    public PlayerRole Role { get; init; }

    public DateTimeOffset JoinedAt { get; init; }

    public DateTimeOffset? LeftAt { get; init; }
}