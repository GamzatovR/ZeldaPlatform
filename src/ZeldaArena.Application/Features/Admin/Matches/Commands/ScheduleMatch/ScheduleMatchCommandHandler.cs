using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.ScheduleMatch;

/// <summary>
/// Матч ставится только между участниками турнира и только в открытый турнир: сетка,
/// в которой играет команда вне состава, противоречит самой себе, а доигранный турнир —
/// это история (docs/adr/ADR-0010). «Команда сама с собой» и чётный формат серии
/// отвергает <c>Match.Schedule</c>.
/// </summary>
public sealed class ScheduleMatchCommandHandler(
    IRepository<Match> matchRepository,
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ScheduleMatchCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ScheduleMatchCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tournament = await queryExecutor.FirstOrDefaultAsync(
            tournaments.Query()
                .Where(item => item.Id == request.TournamentId)
                .Select(item => new
                {
                    item.Status,
                    TeamIds = item.Participants.Select(participant => participant.TeamId).ToList(),
                }),
            cancellationToken);

        if (tournament is null)
        {
            return Result.Failure<Guid>(EsportsErrors.TournamentNotFound);
        }

        if (tournament.Status is Domain.Enums.TournamentStatus.Finished or Domain.Enums.TournamentStatus.Canceled)
        {
            return Result.Failure<Guid>(new Error(
                "tournament.roster_is_closed",
                "Состав участников закрыт: турнир уже завершён или отменён."));
        }

        if (!tournament.TeamIds.Contains(request.TeamAId) || !tournament.TeamIds.Contains(request.TeamBId))
        {
            return Result.Failure<Guid>(EsportsErrors.TeamNotInTournament);
        }

        Match? match = null;

        var scheduled = DomainRules.Apply(() => match = Match.Schedule(
            request.TournamentId,
            request.TeamAId,
            request.TeamBId,
            request.ScheduledAt,
            request.BestOf,
            request.StreamUrl));

        if (scheduled.IsFailure)
        {
            return Result.Failure<Guid>(scheduled.Error);
        }

        await matchRepository.AddAsync(match!, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return match!.Id;
    }
}