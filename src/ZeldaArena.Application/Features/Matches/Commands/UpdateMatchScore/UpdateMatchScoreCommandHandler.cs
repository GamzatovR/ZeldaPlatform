using MediatR;

using ZeldaArena.Application.Common.Exceptions;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

public sealed class UpdateMatchScoreCommandHandler(
    IRepository<Match> matches,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMatchScoreCommand, Result>
{
    public async Task<Result> Handle(
        UpdateMatchScoreCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var match = await matches.GetByIdAsync(request.MatchId, cancellationToken)
            .ConfigureAwait(false);

        if (match is null)
        {
            return Result.Failure(EsportsErrors.MatchNotFound);
        }

        if ((request.ExpectedScoreA is { } expectedA && expectedA != match.ScoreA)
            || (request.ExpectedScoreB is { } expectedB && expectedB != match.ScoreB))
        {
            throw new ConcurrencyConflictException();
        }

        var result = DomainRules.Apply(() => match.UpdateScore(request.ScoreA, request.ScoreB));

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}