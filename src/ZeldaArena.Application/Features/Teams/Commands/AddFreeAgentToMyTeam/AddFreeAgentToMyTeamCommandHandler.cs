using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.AddFreeAgentToMyTeam;

/// <summary>
/// «Игрок не может состоять в двух командах одновременно» (docs/SPEC.md §15) — правило
/// между агрегатами: сущность <c>Team</c> чужих составов не видит, поэтому его проверяет
/// сценарий и отвечает понятной ошибкой. Два одновременных запроса, каждый из которых
/// успел увидеть игрока свободным, упрутся в частичный уникальный индекс
/// <c>IX_RosterEntries_PlayerId_Active</c> — второй не сохранится.
///
/// Проверка в хендлере, а не в валидаторе: отказ валидатора до Фазы 11 превращается
/// в ошибку сервера, а у этого отказа должно быть внятное сообщение на форме.
/// </summary>
public sealed class AddFreeAgentToMyTeamCommandHandler(
    IRepository<Team> teamRepository,
    IReadRepository<Player> players,
    IReadRepository<RosterEntry> rosterEntries,
    IQueryExecutor queryExecutor,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddFreeAgentToMyTeamCommand, Result>
{
    public async Task<Result> Handle(AddFreeAgentToMyTeamCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var access = await MyTeamAccess
            .LoadEditableAsync(teamRepository, entitlements, currentUser, request.TeamId, cancellationToken)
            .ConfigureAwait(false);

        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        if (await players.FindAsync(request.PlayerId, cancellationToken).ConfigureAwait(false) is null)
        {
            return Result.Failure(EsportsErrors.PlayerNotFound);
        }

        if (await queryExecutor
                .AnyAsync(
                    rosterEntries.Query().Where(entry => entry.PlayerId == request.PlayerId && entry.LeftAt == null),
                    cancellationToken)
                .ConfigureAwait(false))
        {
            return Result.Failure(EsportsErrors.PlayerInAnotherTeam);
        }

        access.Value.AddPlayer(request.PlayerId, request.Role, clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}