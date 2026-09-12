using ZeldaArena.Application.Features.Teams;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;

public sealed record TeamEditDto
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

    public int Rating { get; init; }

    public bool IsApproved { get; init; }

    public bool IsUserOwned { get; init; }

    public int MatchCount { get; init; }

    public int TournamentCount { get; init; }

    /// <summary>Все записи состава, включая закрытые: состав историчен (docs/SPEC.md §6).</summary>
    public int RosterEntryCount { get; init; }

    public IReadOnlyList<RosterPlayerDto> Roster { get; init; } = [];

    /// <summary>Игроки, не состоящие сейчас ни в одной команде.</summary>
    public IReadOnlyList<FreeAgentDto> FreeAgents { get; init; } = [];

    /// <summary>
    /// Удалить можно команду без истории: без матчей, без участия в турнирах
    /// и без записей состава (docs/adr/ADR-0010).
    /// </summary>
    public bool CanDelete => MatchCount == 0 && TournamentCount == 0 && RosterEntryCount == 0;
}