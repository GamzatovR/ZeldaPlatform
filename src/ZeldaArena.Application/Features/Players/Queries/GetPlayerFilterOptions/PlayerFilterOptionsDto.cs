namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerFilterOptions;

public sealed record PlayerFilterOptionsDto
{
    public IReadOnlyList<string> Countries { get; init; } = [];

    public IReadOnlyList<TeamOptionDto> Teams { get; init; } = [];
}