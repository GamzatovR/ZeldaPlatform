using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchFormOptions;

public sealed record GetMatchFormOptionsQuery(Guid? TournamentId) : IQuery<MatchFormOptionsDto>;