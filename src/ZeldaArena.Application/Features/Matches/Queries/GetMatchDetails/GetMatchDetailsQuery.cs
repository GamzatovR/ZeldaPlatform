using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

public sealed record GetMatchDetailsQuery(Guid Id) : IQuery<MatchDetailsDto?>;