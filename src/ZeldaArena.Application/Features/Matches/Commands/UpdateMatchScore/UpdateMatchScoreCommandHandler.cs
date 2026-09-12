using MediatR;

using ZeldaArena.Application.Common.Exceptions;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

/// <summary>
/// Хендлер делает ровно одно: находит матч, просит его изменить счёт и сохраняет.
///
/// Проверок формата серии здесь нет — они внутри Match.UpdateScore, единственного
/// способа изменить счёт (docs/CONVENTIONS.md). Рассылки в SignalR здесь тоже нет: сущность
/// поднимает MatchScoreChangedEvent, интерсептор рассылает его после сохранения,
/// обработчик события зовёт IRealtimeNotifier. Это пример SRP из docs/SPEC.md §5.5.
///
/// Отсутствие матча и нарушение инварианта — ожидаемые исходы, поэтому <c>Result</c>:
/// и то и другое достижимо обычным пультом (вкладка, открытая до завершения матча).
///
/// Устаревший пульт ловится сверкой ожидаемого счёта: она отвечает конфликтом ещё
/// до правки, тогда как токен конкурентности (xmin) поймал бы только одновременные
/// сохранения. Оба рубежа нужны — второй модератор мог успеть и сохранить, и уйти.
/// </summary>
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