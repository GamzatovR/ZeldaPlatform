namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;

public sealed record TournamentParticipantDto(
    Guid TeamId,
    string TeamName,
    string TeamSlug,
    int Seed,
    int? Placement,
    int MatchCount);