using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.DeleteTournament;

public sealed class DeleteTournamentCommandHandler(
    IRepository<Tournament> tournamentRepository,
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTournamentCommand, Result>
{
    public async Task<Result> Handle(DeleteTournamentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tournament = await tournamentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (tournament is null)
        {
            return Result.Failure(EsportsErrors.TournamentNotFound);
        }

        if (await queryExecutor.AnyAsync(matches.Query().Where(match => match.TournamentId == request.Id), cancellationToken))
        {
            return Result.Failure(EsportsErrors.TournamentHasMatches);
        }

        var logo = tournament.LogoPath;

        tournamentRepository.Remove(tournament);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logo is not null)
        {
            await storage.DeleteAsync(logo, cancellationToken);
        }

        return Result.Success();
    }
}