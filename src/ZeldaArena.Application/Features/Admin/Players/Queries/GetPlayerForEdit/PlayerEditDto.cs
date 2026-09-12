using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayerForEdit;

public sealed record PlayerEditDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;

    public PlayerRole Role { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public DateOnly? BirthDate { get; init; }

    public string? Bio { get; init; }

    public string? AvatarPath { get; init; }

    public string? TeamName { get; init; }

    /// <summary>Все записи состава, включая закрытые.</summary>
    public int RosterEntryCount { get; init; }

    public int StatsCount { get; init; }

    /// <summary>
    /// Удалить можно игрока без истории: не состоял в командах и не имеет статистики
    /// (docs/adr/ADR-0010).
    /// </summary>
    public bool CanDelete => RosterEntryCount == 0 && StatsCount == 0;
}