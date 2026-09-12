namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;

/// <summary>Команда в выпадающем списке формы.</summary>
public sealed record TeamOptionDto(Guid Id, string Name, string Tag);