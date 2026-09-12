using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.ChangeTournamentStatus;

/// <summary>
/// Недопустимый переход (завершить ещё не начатый, отменить завершённый) отвергает
/// сама сущность; здесь её отказ становится ответом сценария, а не ошибкой сервера.
/// </summary>
public sealed class ChangeTournamentStatusCommandHandler(
    IRepository<Tournament> tournamentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeTournamentStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeTournamentStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tournament = await tournamentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (tournament is null)
        {
            return Result.Failure(EsportsErrors.TournamentNotFound);
        }

        var result = DomainRules.Apply(request.Transition switch
        {
            TournamentTransition.Start => tournament.Start,
            TournamentTransition.Finish => tournament.Finish,
            _ => tournament.Cancel,
        });

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}