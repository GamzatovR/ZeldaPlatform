namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchFormOptions;

public sealed record MatchFormOptionsDto
{
    /// <summary>Анонсированные и идущие турниры: в доигранный матч не поставишь.</summary>
    public IReadOnlyList<AdminTournamentOptionDto> Tournaments { get; init; } = [];

    /// <summary>Участники выбранного турнира; пусто, пока турнир не выбран.</summary>
    public IReadOnlyList<AdminTeamOptionDto> Teams { get; init; } = [];
}