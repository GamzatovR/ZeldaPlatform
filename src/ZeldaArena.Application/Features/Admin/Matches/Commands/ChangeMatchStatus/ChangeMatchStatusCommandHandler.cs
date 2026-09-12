using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.ChangeMatchStatus;

/// <summary>
/// Время старта и завершения берётся с часов приложения, а не из запроса: клиент
/// не должен решать, когда матч начался. Победителя выводит из счёта сама сущность
/// (<c>Match.Finish</c>), поэтому таблица результатов не разойдётся со счётом.
/// </summary>
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