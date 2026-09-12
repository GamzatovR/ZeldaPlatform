using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.DeleteMatch;

public sealed class DeleteMatchCommandHandler(
    IRepository<Match> matches,
    IReadRepository<Match> readMatches,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteMatchCommand, Result>
{
    public async Task<Result> Handle(DeleteMatchCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var match = await matches.GetByIdAsync(request.Id, cancellationToken);

        if (match is null)
        {
            return Result.Failure(EsportsErrors.MatchNotFound);
        }

        var hasStats = await queryExecutor.AnyAsync(
            readMatches.Query()
                .Where(item => item.Id == request.Id)
                .SelectMany(item => item.PlayerStats),
            cancellationToken);

        var untouched = match.Status is MatchStatus.Scheduled or MatchStatus.Postponed
            && match.ScoreA == 0
            && match.ScoreB == 0
            && !hasStats;

        if (!untouched)
        {
            return Result.Failure(EsportsErrors.MatchHasHistory);
        }

        matches.Remove(match);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}