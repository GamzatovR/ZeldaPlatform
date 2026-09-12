using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.ChangeMatchStatus;

public sealed class ChangeMatchStatusCommandHandler(
    IRepository<Match> matches,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeMatchStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeMatchStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var match = await matches.GetByIdAsync(request.Id, cancellationToken);

        if (match is null)
        {
            return Result.Failure(EsportsErrors.MatchNotFound);
        }

        var now = clock.UtcNow;

        var result = DomainRules.Apply(() =>
        {
            switch (request.Transition)
            {
                case MatchTransition.Start:
                    match.Start(now);
                    break;
                case MatchTransition.Finish:
                    match.Finish(now);
                    break;
                default:
                    match.Cancel();
                    break;
            }
        });

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}