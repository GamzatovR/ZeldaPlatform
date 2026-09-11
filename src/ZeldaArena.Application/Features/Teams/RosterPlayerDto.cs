using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Teams;

/// <summary>Игрок текущего состава команды.</summary>
public sealed record RosterPlayerDto
{
    public Guid PlayerId { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public PlayerRole Role { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public string? AvatarPath { get; init; }

    public DateTimeOffset JoinedAt { get; init; }
}