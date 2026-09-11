using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeams;

/// <summary>Карточка команды в списке.</summary>
public sealed record TeamListItemDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Tag { get; init; } = string.Empty;

    public string? LogoPath { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public Region Region { get; init; }

    public int Rating { get; init; }

    /// <summary>Игроков в текущем составе.</summary>
    public int PlayerCount { get; init; }
}