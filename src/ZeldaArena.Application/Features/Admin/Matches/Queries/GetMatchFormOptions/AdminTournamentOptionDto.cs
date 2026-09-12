namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchFormOptions;

/// <summary>Турнир в выпадающем списке админки — по идентификатору, а не по слагу.</summary>
public sealed record AdminTournamentOptionDto(Guid Id, string Name);