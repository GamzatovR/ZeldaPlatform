using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

/// <summary>Игрок в составе команды на момент матча.</summary>
public sealed record LineupPlayerDto
{
    public Guid PlayerId { get; init; }

    public Guid TeamId { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public PlayerRole Role { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public string? AvatarPath { get; init; }
}