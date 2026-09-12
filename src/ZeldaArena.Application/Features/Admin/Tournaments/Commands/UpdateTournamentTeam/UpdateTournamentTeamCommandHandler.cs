using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournamentTeam;

public sealed class UpdateTournamentTeamCommandHandler(
    IRepository<Tournament> tournamentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTournamentTeamCommand, Result>
{
    public async Task<Result> Handle(UpdateTournamentTeamCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tournament = await tournamentRepository.GetByIdAsync(request.TournamentId, cancellationToken);

        if (tournament is null)
        {
            return Result.Failure(EsportsErrors.TournamentNotFound);
        }

        var result = DomainRules.Apply(() =>
        {
            tournament.ChangeSeed(request.TeamId, request.Seed);

            if (request.Placement is { } placement)
            {
                tournament.SetPlacement(request.TeamId, placement);
            }
        });

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}