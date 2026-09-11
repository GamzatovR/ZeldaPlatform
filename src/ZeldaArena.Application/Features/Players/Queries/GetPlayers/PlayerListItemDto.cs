using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayers;

/// <summary>Карточка игрока в списке.</summary>
public sealed record PlayerListItemDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public PlayerRole Role { get; init; }

    public string? AvatarPath { get; init; }

    /// <summary>Текущая одобренная команда; <c>null</c> — свободный игрок.</summary>
    public Guid? TeamId { get; init; }

    public string? TeamName { get; init; }

    public string? TeamSlug { get; init; }
}