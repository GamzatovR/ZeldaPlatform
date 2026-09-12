using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentOptions;

public sealed record GetTournamentOptionsQuery : IQuery<IReadOnlyList<TournamentOptionDto>>;