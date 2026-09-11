using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentOptions;

/// <summary>Свежие турниры первыми: чаще всего ищут идущий или ближайший.</summary>
public sealed class GetTournamentOptionsQueryHandler(
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTournamentOptionsQuery, IReadOnlyList<TournamentOptionDto>>
{
    public Task<IReadOnlyList<TournamentOptionDto>> Handle(
        GetTournamentOptionsQuery request,
        CancellationToken cancellationToken) =>
        queryExecutor.ToListAsync(
            tournaments.Query()
                .OrderByDescending(tournament => tournament.StartsAt)
                .Select(tournament => new TournamentOptionDto(tournament.Slug.Value, tournament.Name)),
            cancellationToken);
}