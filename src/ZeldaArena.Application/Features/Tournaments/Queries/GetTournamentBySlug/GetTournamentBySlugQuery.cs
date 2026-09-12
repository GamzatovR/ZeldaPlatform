using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

public sealed record GetTournamentBySlugQuery(string Slug) : IQuery<TournamentDetailsDto?>;