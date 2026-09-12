using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayersForAdmin;

public sealed record AdminPlayerRowDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public string? FullName { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public PlayerRole Role { get; init; }

    /// <summary>Действующая команда или <see langword="null"/> у свободного игрока.</summary>
    public string? TeamName { get; init; }
}