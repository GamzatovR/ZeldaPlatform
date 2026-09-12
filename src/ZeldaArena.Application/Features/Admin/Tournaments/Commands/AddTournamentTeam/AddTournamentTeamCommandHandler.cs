using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.AddTournamentTeam;

/// <summary>
/// Участвовать может только одобренная команда: неодобренная скрыта со всех публичных
/// страниц (docs/adr/ADR-0008), и в сетке турнира она появилась бы в обход модерации.
/// Повтор и закрытый состав отвергает сущность.
/// </summary>
public sealed class AddTournamentTeamCommandHandler(
    IRepository<Tournament> tournamentRepository,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddTournamentTeamCommand, Result>
{
    public async Task<Result> Handle(AddTournamentTeamCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tournament = await tournamentRepository.GetByIdAsync(request.TournamentId, cancellationToken);

        if (tournament is null)
        {
            return Result.Failure(EsportsErrors.TournamentNotFound);
        }

        var team = await queryExecutor.FirstOrDefaultAsync(
            teams.Query()
                .Where(item => item.Id == request.TeamId)
                .Select(item => new { item.IsApproved }),
            cancellationToken);

        if (team is null)
        {
            return Result.Failure(EsportsErrors.TeamNotFound);
        }

        if (!team.IsApproved)
        {
            return Result.Failure(EsportsErrors.TeamNotApproved);
        }

        var result = DomainRules.Apply(() => tournament.AddTeam(request.TeamId, request.Seed));

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}