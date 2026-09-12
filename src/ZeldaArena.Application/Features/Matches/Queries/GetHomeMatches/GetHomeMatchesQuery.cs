using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;

public sealed record GetHomeMatchesQuery(int Count = 4) : IQuery<IReadOnlyList<MatchCardDto>>;