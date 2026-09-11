using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;

public sealed record MyTeamDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Tag { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;

    public Region Region { get; init; }

    public DateOnly? FoundedAt { get; init; }

    public string? Description { get; init; }

    public string? LogoPath { get; init; }

    public bool IsApproved { get; init; }

    public int Rating { get; init; }

    /// <summary>
    /// Есть ли сейчас право править команду. Отказ показывается заранее, а не после
    /// отправки формы, — но сценарии правки проверяют право сами.
    /// </summary>
    public bool CanEdit { get; init; }

    public IReadOnlyList<RosterPlayerDto> Roster { get; init; } = [];

    /// <summary>Все свои команды — для переключения, когда их больше одной.</summary>
    public IReadOnlyList<OwnedTeamDto> OwnedTeams { get; init; } = [];
}