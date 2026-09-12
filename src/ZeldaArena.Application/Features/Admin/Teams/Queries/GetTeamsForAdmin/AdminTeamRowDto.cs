using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;

public sealed record AdminTeamRowDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Tag { get; init; } = string.Empty;

    public Region Region { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public int Rating { get; init; }

    public bool IsApproved { get; init; }

    /// <summary>Команда, созданная подписчиком: у неё есть владелец.</summary>
    public bool IsUserOwned { get; init; }

    public int RosterCount { get; init; }

    public int MatchCount { get; init; }
}