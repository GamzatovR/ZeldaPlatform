namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;

/// <summary>
/// Участник турнира. Число матчей команды в этом турнире решает, можно ли её убрать:
/// матч без участника в составе турнира — нарушенная история.
/// </summary>
public sealed record TournamentParticipantDto(
    Guid TeamId,
    string TeamName,
    string TeamSlug,
    int Seed,
    int? Placement,
    int MatchCount);