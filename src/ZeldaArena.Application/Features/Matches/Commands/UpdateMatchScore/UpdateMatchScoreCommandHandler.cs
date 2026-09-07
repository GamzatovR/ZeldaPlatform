using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

/// <summary>
/// Хендлер делает ровно одно: находит матч, просит его изменить счёт и сохраняет.
///
/// Проверок формата серии здесь нет — они внутри Match.UpdateScore, единственного
/// способа изменить счёт (CLAUDE.md). Рассылки в SignalR здесь тоже нет: сущность
/// поднимает MatchScoreChangedEvent, интерсептор рассылает его после сохранения,
/// обработчик события зовёт IRealtimeNotifier. Это пример SRP из docs/SPEC.md §5.5.
///
/// Отсутствие матча — ожидаемый исход, поэтому Result, а не исключение. Нарушение
/// инварианта, наоборот, исключение: значит, до сущности дошёл запрос, который вообще
/// не должен был дойти. В Фазе 11 оно превратится в локализованный ответ 409 (§14.1).
/// </summary>
public sealed class UpdateMatchScoreCommandHandler(
    IRepository<Match> matches,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMatchScoreCommand, Result>
{
    public static readonly Error MatchNotFound =
        new("match.not_found", "Матч не найден.");

    public async Task<Result> Handle(
        UpdateMatchScoreCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var match = await matches.GetByIdAsync(request.MatchId, cancellationToken)
            .ConfigureAwait(false);

        if (match is null)
        {
            return Result.Failure(MatchNotFound);
        }

        match.UpdateScore(request.ScoreA, request.ScoreB);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}